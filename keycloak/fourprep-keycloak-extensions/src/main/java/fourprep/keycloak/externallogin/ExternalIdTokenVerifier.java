package fourprep.keycloak.externallogin;

import java.nio.charset.StandardCharsets;
import java.util.Optional;

import org.jboss.logging.Logger;
import org.keycloak.broker.oidc.OIDCIdentityProviderConfig;
import org.keycloak.crypto.KeyWrapper;
import org.keycloak.crypto.SignatureProvider;
import org.keycloak.jose.jws.JWSInput;
import org.keycloak.jose.jws.JWSInputException;
import org.keycloak.keys.loader.PublicKeyStorageManager;
import org.keycloak.models.KeycloakSession;
import org.keycloak.models.RealmModel;
import org.keycloak.representations.JsonWebToken;

final class ExternalIdTokenVerifier {

    private static final Logger LOGGER = Logger.getLogger(ExternalIdTokenVerifier.class);

    private final KeycloakSession session;
    private final RealmModel realm;

    ExternalIdTokenVerifier(KeycloakSession session, RealmModel realm) {
        this.session = session;
        this.realm = realm;
    }

    Optional<ExternalIdentity> verify(OIDCIdentityProviderConfig config, String encodedToken) {
        if (!config.isValidateSignature()) {
            LOGGER.warnf("Identity provider '%s' must have signature validation enabled", config.getAlias());
            return Optional.empty();
        }

        JWSInput jws;
        JsonWebToken token;
        try {
            jws = new JWSInput(encodedToken);
            token = jws.readJsonContent(JsonWebToken.class);
        } catch (JWSInputException exception) {
            LOGGER.debug("External identity token is not a valid JWS", exception);
            return Optional.empty();
        }

        if (!hasValidSignature(config, jws)) {
            return Optional.empty();
        }

        if (!ExternalIdTokenClaims.isAcceptable(token, config.getIssuer(), config.getClientId(), config.getAllowedClockSkew())) {
            LOGGER.debugf("External identity token claims were rejected for identity provider '%s'", config.getAlias());
            return Optional.empty();
        }

        return Optional.of(ExternalIdTokenClaims.toIdentity(token));
    }

    private boolean hasValidSignature(OIDCIdentityProviderConfig config, JWSInput jws) {
        String algorithm = jws.getHeader().getRawAlgorithm();
        KeyWrapper key = PublicKeyStorageManager.getIdentityProviderKeyWrapper(session, realm, config, jws);
        if (key == null || (key.getAlgorithm() != null && !key.getAlgorithm().equals(algorithm))) {
            LOGGER.debugf("No matching signing key for algorithm '%s'", algorithm);
            return false;
        }

        SignatureProvider signatureProvider = session.getProvider(SignatureProvider.class, algorithm);
        if (signatureProvider == null || !signatureProvider.isAsymmetricAlgorithm()) {
            LOGGER.debugf("Unsupported signature algorithm '%s'", algorithm);
            return false;
        }

        try {
            return signatureProvider.verifier(key).verify(
                    jws.getEncodedSignatureInput().getBytes(StandardCharsets.UTF_8),
                    jws.getSignature());
        } catch (Exception exception) {
            LOGGER.debug("External identity token signature verification failed", exception);
            return false;
        }
    }
}
