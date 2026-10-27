package fourprep.keycloak.externallogin;

import java.util.Arrays;
import java.util.Optional;
import java.util.Set;
import java.util.stream.Collectors;
import java.util.stream.Stream;

import org.keycloak.models.Constants;

final class ExternalSignInRoles {

    static final String CONFIG_PROPERTY = "allowedRoles";

    private final Set<String> allowed;

    private ExternalSignInRoles(Set<String> allowed) {
        this.allowed = allowed;
    }

    static Optional<ExternalSignInRoles> parse(String configValue) {
        if (configValue == null) {
            return Optional.empty();
        }

        Set<String> roles = Arrays.stream(configValue.split(Constants.CFG_DELIMITER))
                .map(String::trim)
                .filter(role -> !role.isEmpty())
                .collect(Collectors.toUnmodifiableSet());

        return roles.isEmpty() ? Optional.empty() : Optional.of(new ExternalSignInRoles(roles));
    }

    boolean permits(Stream<String> accountRoles) {
        return accountRoles.allMatch(allowed::contains);
    }
}
