package fourprep.keycloak.externallogin;

import java.util.Locale;
import java.util.Optional;
import java.util.stream.Stream;

import org.jboss.logging.Logger;
import org.keycloak.credential.CredentialModel;
import org.keycloak.models.FederatedIdentityModel;
import org.keycloak.models.GroupModel;
import org.keycloak.models.KeycloakSession;
import org.keycloak.models.RealmModel;
import org.keycloak.models.RoleModel;
import org.keycloak.models.SubjectCredentialManager;
import org.keycloak.models.UserModel;
import org.keycloak.models.UserProvider;
import org.keycloak.models.UserSessionModel;
import org.keycloak.models.UserSessionProvider;
import org.keycloak.models.credential.PasswordCredentialModel;

final class ExternalUserResolver {

    private static final Logger LOGGER = Logger.getLogger(ExternalUserResolver.class);
    private static final String USERNAME_SEPARATOR = "-";

    private final RealmModel realm;
    private final UserProvider users;
    private final UserSessionProvider sessions;
    private final ExternalSignInRoles allowedRoles;

    ExternalUserResolver(KeycloakSession session, RealmModel realm, ExternalSignInRoles allowedRoles) {
        this.realm = realm;
        this.users = session.users();
        this.sessions = session.sessions();
        this.allowedRoles = allowedRoles;
    }

    UserModel resolve(String providerAlias, ExternalIdentity identity) {
        UserModel linkedUser = users.getUserByFederatedIdentity(
                realm,
                new FederatedIdentityModel(providerAlias, identity.subject(), null));
        if (linkedUser != null) {
            return requireAllowedRoles(linkedUser);
        }

        UserModel user = findByVerifiedEmail(identity)
                .map(this::requireAllowedRoles)
                .map(this::claimIfEmailUnverified)
                .orElseGet(() -> createUser(providerAlias, identity));

        users.addFederatedIdentity(
                realm,
                user,
                new FederatedIdentityModel(providerAlias, identity.subject(), identity.email()));

        return user;
    }

    private UserModel requireAllowedRoles(UserModel user) {
        if (!allowedRoles.permits(accountRoleNames(user))) {
            throw new ExternalSignInNotAllowedException(user.getId());
        }

        return user;
    }

    private Stream<String> accountRoleNames(UserModel user) {
        RoleModel defaultRole = realm.getDefaultRole();
        Stream<RoleModel> groupRoles = user.getGroupsStream().flatMap(GroupModel::getRealmRoleMappingsStream);

        return Stream.concat(user.getRealmRoleMappingsStream(), groupRoles)
                .filter(role -> !isDefaultRole(defaultRole, role))
                .map(RoleModel::getName);
    }

    private static boolean isDefaultRole(RoleModel defaultRole, RoleModel role) {
        return defaultRole != null && (defaultRole.getId().equals(role.getId()) || defaultRole.hasRole(role));
    }

    private Optional<UserModel> findByVerifiedEmail(ExternalIdentity identity) {
        if (!identity.hasVerifiedEmail()) {
            return Optional.empty();
        }

        return Optional.ofNullable(users.getUserByEmail(realm, identity.email()));
    }

    private UserModel claimIfEmailUnverified(UserModel user) {
        if (user.isEmailVerified()) {
            return user;
        }

        removePasswords(user);
        endSessions(user);
        user.setEmailVerified(true);

        LOGGER.infof(
                "User %s was claimed through a verified external email; its password and sessions were removed",
                user.getId());

        return user;
    }

    private static void removePasswords(UserModel user) {
        SubjectCredentialManager credentials = user.credentialManager();
        credentials.getStoredCredentialsByTypeStream(PasswordCredentialModel.TYPE)
                .map(CredentialModel::getId)
                .toList()
                .forEach(credentials::removeStoredCredentialById);
    }

    private void endSessions(UserModel user) {
        for (UserSessionModel userSession : sessions.getUserSessionsStream(realm, user).toList()) {
            sessions.removeUserSession(realm, userSession);
        }

        for (UserSessionModel offlineSession : sessions.getOfflineUserSessionsStream(realm, user).toList()) {
            sessions.removeOfflineUserSession(realm, offlineSession);
        }
    }

    private UserModel createUser(String providerAlias, ExternalIdentity identity) {
        String username = (providerAlias + USERNAME_SEPARATOR + identity.subject()).toLowerCase(Locale.ROOT);
        UserModel user = users.addUser(realm, null, username, true, false);
        user.setEnabled(true);

        if (identity.hasVerifiedEmail()) {
            user.setEmail(identity.email());
            user.setEmailVerified(true);
        }

        user.setFirstName(identity.givenName());
        user.setLastName(identity.familyName());

        return user;
    }
}
