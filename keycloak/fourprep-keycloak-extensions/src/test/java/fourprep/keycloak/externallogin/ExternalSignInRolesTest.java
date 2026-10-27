package fourprep.keycloak.externallogin;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;

import java.util.stream.Stream;

import org.junit.jupiter.api.Test;

class ExternalSignInRolesTest {

    private static final ExternalSignInRoles USERS_ONLY = ExternalSignInRoles.parse("student##parent").orElseThrow();

    @Test
    void permitsAnAccountWithoutRoles() {
        assertTrue(USERS_ONLY.permits(Stream.empty()));
    }

    @Test
    void permitsAnAccountHoldingOnlyAllowedRoles() {
        assertTrue(USERS_ONLY.permits(Stream.of("student")));
        assertTrue(USERS_ONLY.permits(Stream.of("parent", "student")));
    }

    @Test
    void refusesAnAccountHoldingAStaffRole() {
        assertFalse(USERS_ONLY.permits(Stream.of("mentor")));
        assertFalse(USERS_ONLY.permits(Stream.of("super_admin")));
    }

    @Test
    void refusesAnAccountThatAlsoHoldsAStaffRole() {
        assertFalse(USERS_ONLY.permits(Stream.of("student", "teacher")));
    }

    @Test
    void ignoresBlanksAroundConfiguredRoles() {
        assertTrue(ExternalSignInRoles.parse(" student ## parent ").orElseThrow().permits(Stream.of("parent")));
    }

    @Test
    void treatsMissingOrEmptyConfigurationAsNotConfigured() {
        assertTrue(ExternalSignInRoles.parse(null).isEmpty());
        assertTrue(ExternalSignInRoles.parse("").isEmpty());
        assertTrue(ExternalSignInRoles.parse("##").isEmpty());
    }
}
