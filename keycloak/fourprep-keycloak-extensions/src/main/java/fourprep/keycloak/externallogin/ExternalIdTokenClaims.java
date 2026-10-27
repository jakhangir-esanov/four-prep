package fourprep.keycloak.externallogin;

import org.keycloak.representations.JsonWebToken;

final class ExternalIdTokenClaims {

    private static final String EMAIL_CLAIM = "email";
    private static final String EMAIL_VERIFIED_CLAIM = "email_verified";
    private static final String GIVEN_NAME_CLAIM = "given_name";
    private static final String FAMILY_NAME_CLAIM = "family_name";
    private static final String HTTPS_SCHEME = "https://";

    private ExternalIdTokenClaims() {
    }

    static boolean isAcceptable(JsonWebToken token, String trustedIssuer, String audience, int allowedClockSkew) {
        return isPresent(token.getSubject())
                && isTrustedIssuer(token.getIssuer(), trustedIssuer)
                && isPresent(audience)
                && token.hasAudience(audience)
                && token.getExp() != null
                && token.isActive(allowedClockSkew);
    }

    static ExternalIdentity toIdentity(JsonWebToken token) {
        return new ExternalIdentity(
                token.getSubject(),
                readString(token, EMAIL_CLAIM),
                Boolean.parseBoolean(readString(token, EMAIL_VERIFIED_CLAIM)),
                readString(token, GIVEN_NAME_CLAIM),
                readString(token, FAMILY_NAME_CLAIM));
    }

    private static boolean isTrustedIssuer(String issuer, String trustedIssuer) {
        return isPresent(trustedIssuer)
                && isPresent(issuer)
                && (trustedIssuer.equals(issuer) || trustedIssuer.equals(HTTPS_SCHEME + issuer));
    }

    private static String readString(JsonWebToken token, String claim) {
        Object value = token.getOtherClaims().get(claim);
        return value == null ? null : value.toString();
    }

    private static boolean isPresent(String value) {
        return value != null && !value.isBlank();
    }
}
