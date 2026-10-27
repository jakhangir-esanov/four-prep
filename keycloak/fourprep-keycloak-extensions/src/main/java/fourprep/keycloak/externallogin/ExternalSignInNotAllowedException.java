package fourprep.keycloak.externallogin;

final class ExternalSignInNotAllowedException extends RuntimeException {

    ExternalSignInNotAllowedException(String userId) {
        super("User " + userId + " holds a role that may not use external sign-in");
    }
}
