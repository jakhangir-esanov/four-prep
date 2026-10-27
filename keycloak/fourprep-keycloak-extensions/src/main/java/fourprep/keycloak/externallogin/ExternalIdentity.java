package fourprep.keycloak.externallogin;

record ExternalIdentity(
        String subject,
        String email,
        boolean emailVerified,
        String givenName,
        String familyName) {

    boolean hasVerifiedEmail() {
        return emailVerified && email != null && !email.isBlank();
    }
}
