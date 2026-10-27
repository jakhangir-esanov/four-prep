package fourprep.keycloak.externallogin;

import java.util.List;
import java.util.Optional;

import jakarta.ws.rs.core.MultivaluedMap;
import jakarta.ws.rs.core.Response;
import org.jboss.logging.Logger;
import org.keycloak.authentication.AuthenticationFlowContext;
import org.keycloak.authentication.AuthenticationFlowError;
import org.keycloak.authentication.authenticators.directgrant.AbstractDirectGrantAuthenticator;
import org.keycloak.broker.oidc.OIDCIdentityProviderConfig;
import org.keycloak.events.Details;
import org.keycloak.events.Errors;
import org.keycloak.models.AuthenticationExecutionModel.Requirement;
import org.keycloak.models.AuthenticatorConfigModel;
import org.keycloak.models.IdentityProviderModel;
import org.keycloak.models.KeycloakSession;
import org.keycloak.models.ModelDuplicateException;
import org.keycloak.models.RealmModel;
import org.keycloak.models.UserModel;
import org.keycloak.provider.ProviderConfigProperty;

public final class ExternalIdTokenAuthenticator extends AbstractDirectGrantAuthenticator {

    public static final String PROVIDER_ID = "fourprep-external-id-token";

    static final String PROVIDER_PARAMETER = "external_provider";
    static final String ID_TOKEN_PARAMETER = "external_id_token";

    static final String NOT_ALLOWED_DESCRIPTION = "Account is not allowed to use external sign-in";

    private static final Logger LOGGER = Logger.getLogger(ExternalIdTokenAuthenticator.class);
    private static final String INVALID_GRANT = "invalid_grant";
    private static final Requirement[] REQUIREMENT_CHOICES = {Requirement.ALTERNATIVE, Requirement.DISABLED};
    private static final List<ProviderConfigProperty> CONFIG_PROPERTIES = List.of(new ProviderConfigProperty(
            ExternalSignInRoles.CONFIG_PROPERTY,
            "Allowed roles",
            "Realm roles an account may hold and still sign in with an external ID token. An account holding any other "
                    + "role, staff for example, is refused before it is linked. Without this setting every external "
                    + "sign-in is refused.",
            ProviderConfigProperty.MULTIVALUED_STRING_TYPE,
            null));

    @Override
    public void authenticate(AuthenticationFlowContext context) {
        MultivaluedMap<String, String> form = context.getHttpRequest().getDecodedFormParameters();
        String providerAlias = form.getFirst(PROVIDER_PARAMETER);
        String idToken = form.getFirst(ID_TOKEN_PARAMETER);

        if (isBlank(providerAlias) && isBlank(idToken)) {
            context.attempted();
            return;
        }

        if (isBlank(providerAlias) || isBlank(idToken)) {
            reject(context, AuthenticationFlowError.INVALID_CREDENTIALS, Errors.INVALID_REQUEST,
                    "Both external_provider and external_id_token are required");
            return;
        }

        context.getEvent().detail(Details.IDENTITY_PROVIDER, providerAlias);

        IdentityProviderModel provider = context.getSession().identityProviders().getByAlias(providerAlias);
        if (provider == null || !provider.isEnabled()) {
            reject(context, AuthenticationFlowError.IDENTITY_PROVIDER_NOT_FOUND, Errors.UNKNOWN_IDENTITY_PROVIDER,
                    "Unknown identity provider");
            return;
        }

        Optional<ExternalSignInRoles> allowedRoles = readAllowedRoles(context);
        if (allowedRoles.isEmpty()) {
            LOGGER.warnf("Execution of %s has no %s configured, so every external sign-in is refused",
                    PROVIDER_ID, ExternalSignInRoles.CONFIG_PROPERTY);
            reject(context, AuthenticationFlowError.INTERNAL_ERROR, Errors.IDENTITY_PROVIDER_ERROR,
                    "External sign-in is not configured");
            return;
        }

        Optional<ExternalIdentity> identity = new ExternalIdTokenVerifier(context.getSession(), context.getRealm())
                .verify(new OIDCIdentityProviderConfig(provider), idToken);
        if (identity.isEmpty()) {
            reject(context, AuthenticationFlowError.INVALID_CREDENTIALS, Errors.INVALID_TOKEN,
                    "Invalid external identity token");
            return;
        }

        authenticateUser(context, providerAlias, identity.get(), allowedRoles.get());
    }

    private static Optional<ExternalSignInRoles> readAllowedRoles(AuthenticationFlowContext context) {
        return Optional.ofNullable(context.getAuthenticatorConfig())
                .map(AuthenticatorConfigModel::getConfig)
                .map(config -> config.get(ExternalSignInRoles.CONFIG_PROPERTY))
                .flatMap(ExternalSignInRoles::parse);
    }

    private void authenticateUser(
            AuthenticationFlowContext context,
            String providerAlias,
            ExternalIdentity identity,
            ExternalSignInRoles allowedRoles) {
        context.getEvent().detail(Details.IDENTITY_PROVIDER_USER_ID, identity.subject());

        UserModel user;
        try {
            user = new ExternalUserResolver(context.getSession(), context.getRealm(), allowedRoles)
                    .resolve(providerAlias, identity);
        } catch (ExternalSignInNotAllowedException exception) {
            LOGGER.info(exception.getMessage());
            reject(context, AuthenticationFlowError.INVALID_USER, Errors.NOT_ALLOWED, NOT_ALLOWED_DESCRIPTION);
            return;
        } catch (ModelDuplicateException exception) {
            reject(context, AuthenticationFlowError.INVALID_USER, Errors.IDENTITY_PROVIDER_ERROR,
                    "External account could not be linked");
            return;
        }

        context.getEvent().user(user);

        if (!user.isEnabled()) {
            reject(context, AuthenticationFlowError.USER_DISABLED, Errors.USER_DISABLED, "Account disabled");
            return;
        }

        context.setUser(user);
        context.success();
    }

    private void reject(AuthenticationFlowContext context, AuthenticationFlowError flowError, String eventError, String description) {
        context.getEvent().error(eventError);
        Response response = errorResponse(Response.Status.UNAUTHORIZED.getStatusCode(), INVALID_GRANT, description);
        context.failure(flowError, response);
    }

    private static boolean isBlank(String value) {
        return value == null || value.isBlank();
    }

    @Override
    public boolean requiresUser() {
        return false;
    }

    @Override
    public boolean configuredFor(KeycloakSession session, RealmModel realm, UserModel user) {
        return true;
    }

    @Override
    public void setRequiredActions(KeycloakSession session, RealmModel realm, UserModel user) {
    }

    @Override
    public boolean isUserSetupAllowed() {
        return false;
    }

    @Override
    public String getDisplayType() {
        return "FourPrep External ID Token";
    }

    @Override
    public String getReferenceCategory() {
        return "external-id-token";
    }

    @Override
    public boolean isConfigurable() {
        return true;
    }

    @Override
    public Requirement[] getRequirementChoices() {
        return REQUIREMENT_CHOICES;
    }

    @Override
    public String getHelpText() {
        return "Signs in a user from a Google ID token verified against a configured identity provider.";
    }

    @Override
    public List<ProviderConfigProperty> getConfigProperties() {
        return CONFIG_PROPERTIES;
    }

    @Override
    public String getId() {
        return PROVIDER_ID;
    }
}
