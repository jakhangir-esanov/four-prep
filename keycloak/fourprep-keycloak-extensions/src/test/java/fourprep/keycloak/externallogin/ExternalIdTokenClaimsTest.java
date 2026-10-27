package fourprep.keycloak.externallogin;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNull;
import static org.junit.jupiter.api.Assertions.assertTrue;

import org.junit.jupiter.api.Test;
import org.keycloak.common.util.Time;
import org.keycloak.representations.JsonWebToken;

class ExternalIdTokenClaimsTest {

    private static final String GOOGLE_ISSUER = "https://accounts.google.com";
    private static final String WEB_CLIENT_ID = "web-client.apps.googleusercontent.com";
    private static final String ANDROID_CLIENT_ID = "android-client.apps.googleusercontent.com";
    private static final int NO_CLOCK_SKEW = 0;

    @Test
    void acceptsTokenIssuedForTheConfiguredAudience() {
        assertTrue(ExternalIdTokenClaims.isAcceptable(validToken(), GOOGLE_ISSUER, WEB_CLIENT_ID, NO_CLOCK_SKEW));
    }

    @Test
    void acceptsTokenPresentedByAnotherClientOfTheSameProject() {
        JsonWebToken token = validToken();
        token.issuedFor(ANDROID_CLIENT_ID);

        assertTrue(ExternalIdTokenClaims.isAcceptable(token, GOOGLE_ISSUER, WEB_CLIENT_ID, NO_CLOCK_SKEW));
    }

    @Test
    void rejectsUntrustedIssuer() {
        JsonWebToken token = validToken();
        token.issuer("https://evil.example.com");

        assertFalse(ExternalIdTokenClaims.isAcceptable(token, GOOGLE_ISSUER, WEB_CLIENT_ID, NO_CLOCK_SKEW));
    }

    @Test
    void acceptsGoogleIssuerWithoutScheme() {
        JsonWebToken token = validToken();
        token.issuer("accounts.google.com");

        assertTrue(ExternalIdTokenClaims.isAcceptable(token, GOOGLE_ISSUER, WEB_CLIENT_ID, NO_CLOCK_SKEW));
    }

    @Test
    void rejectsUntrustedIssuerWithoutScheme() {
        JsonWebToken token = validToken();
        token.issuer("evil.example.com");

        assertFalse(ExternalIdTokenClaims.isAcceptable(token, GOOGLE_ISSUER, WEB_CLIENT_ID, NO_CLOCK_SKEW));
    }

    @Test
    void rejectsTokenWithoutIssuer() {
        JsonWebToken token = validToken();
        token.issuer(null);

        assertFalse(ExternalIdTokenClaims.isAcceptable(token, GOOGLE_ISSUER, WEB_CLIENT_ID, NO_CLOCK_SKEW));
    }

    @Test
    void rejectsWhenNoIssuerIsConfigured() {
        assertFalse(ExternalIdTokenClaims.isAcceptable(validToken(), null, WEB_CLIENT_ID, NO_CLOCK_SKEW));
    }

    @Test
    void rejectsTokenIssuedForAnotherAudience() {
        assertFalse(ExternalIdTokenClaims.isAcceptable(validToken(), GOOGLE_ISSUER, ANDROID_CLIENT_ID, NO_CLOCK_SKEW));
    }

    @Test
    void rejectsExpiredToken() {
        JsonWebToken token = validToken();
        token.exp((long) Time.currentTime() - 60);

        assertFalse(ExternalIdTokenClaims.isAcceptable(token, GOOGLE_ISSUER, WEB_CLIENT_ID, NO_CLOCK_SKEW));
    }

    @Test
    void rejectsTokenWithoutExpiration() {
        JsonWebToken token = validToken();
        token.exp(null);

        assertFalse(ExternalIdTokenClaims.isAcceptable(token, GOOGLE_ISSUER, WEB_CLIENT_ID, NO_CLOCK_SKEW));
    }

    @Test
    void rejectsTokenWithoutSubject() {
        JsonWebToken token = validToken();
        token.subject(null);

        assertFalse(ExternalIdTokenClaims.isAcceptable(token, GOOGLE_ISSUER, WEB_CLIENT_ID, NO_CLOCK_SKEW));
    }

    @Test
    void readsBooleanEmailVerifiedClaim() {
        JsonWebToken token = validToken();
        token.setOtherClaims("email", "ali@gmail.com");
        token.setOtherClaims("email_verified", true);

        ExternalIdentity identity = ExternalIdTokenClaims.toIdentity(token);

        assertEquals("ali@gmail.com", identity.email());
        assertTrue(identity.hasVerifiedEmail());
    }

    @Test
    void treatsUnverifiedEmailAsUnverified() {
        JsonWebToken token = validToken();
        token.setOtherClaims("email", "ali@gmail.com");
        token.setOtherClaims("email_verified", false);

        assertFalse(ExternalIdTokenClaims.toIdentity(token).hasVerifiedEmail());
    }

    @Test
    void leavesMissingProfileClaimsEmpty() {
        ExternalIdentity identity = ExternalIdTokenClaims.toIdentity(validToken());

        assertEquals("108390896687407519623", identity.subject());
        assertNull(identity.email());
        assertFalse(identity.hasVerifiedEmail());
        assertNull(identity.givenName());
    }

    private static JsonWebToken validToken() {
        JsonWebToken token = new JsonWebToken();
        token.issuer("https://accounts.google.com");
        token.subject("108390896687407519623");
        token.audience(WEB_CLIENT_ID);
        token.issuedNow();
        token.exp((long) Time.currentTime() + 3600);
        return token;
    }
}
