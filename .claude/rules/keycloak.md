---
paths:
  - "keycloak/**"
  - "docker-compose.yml"
  - "src/Modules/Auth/FourPrep.Modules.Auth.Infrastructure/Identity/**/*.cs"
---

# Keycloak rules

## Realm file (`keycloak/realms/fourprep-realm.json`)

- It is the source of truth for the realm: roles, the `fourprep-api` client and its mappers, token
  lifetimes, locales, SMTP, User Profile, the `google` identity provider, the `fourprep direct grant`
  flow, the authenticator config and the direct grant binding.
- It is imported only when the realm does not exist yet. A change reaches an existing realm in one of
  two ways: recreate the `fourprep_keycloak` database (ask the user first: it deletes every local
  account), or apply the same change through the Admin API and compare the live realm with the file.
  Never leave the live realm and the file different.
- Keep it formatted as `json.dumps(..., indent=2, ensure_ascii=False)` with LF endings, so a change
  shows as a small diff.
- A new user attribute must be declared in the User Profile, or Keycloak drops it silently. If it must
  reach the token, add a protocol mapper too.
- Environment values use import placeholders: `${GOOGLE_CLIENT_ID:not-configured}`. Real values live
  in `.env` (git-ignored) and reach the container through `docker-compose.yml`.

## Extension (`keycloak/fourprep-keycloak-extensions`)

- Java 21, Maven, Keycloak SPI jars `provided`. `keycloak/Dockerfile` builds it, runs its tests and
  copies the jar into the image; `docker compose up -d --build` rebuilds it.
- It implements the internal authenticator SPI (`KC-SERVICES0047` at startup is expected). On every
  Keycloak version bump: update `keycloak.version` in `pom.xml` and the base image together, rebuild,
  and retest password login, Google sign-in, refresh and staff refusal.
- Pure logic goes in classes without Keycloak models (`ExternalIdTokenClaims`, `ExternalSignInRoles`)
  and gets JUnit tests. Code that needs a running Keycloak is proven on a throwaway container, never
  on the user's local realm.
- The error descriptions the API maps (`Account is not allowed to use external sign-in`) are a
  contract with `KeycloakAdminClient`; change both together.
- Everything the authenticator needs to decide is configured on its execution in the realm file
  (`allowedRoles`), not hard-coded. Missing configuration refuses every external sign-in.
- No comments in Java either.

## Testing against Keycloak

- Use a separate container on another port (`-p 8081:8080`, dev H2 database) with the realm folder
  mounted, never the user's `fourprep-keycloak`. Point the API at it through environment variables and
  a throwaway database, and remove both afterwards.
- A Google token cannot be produced locally. Point the test realm's `google` provider at a local
  signing key (`useJwksUrl=false`, `publicKeySignatureVerifier`) and sign tokens with that key.
