namespace FourPrep.Modules.Auth.Infrastructure.Identity;

using System.Net;
using System.Text;
using Newtonsoft.Json;
using FourPrep.Common.Domain;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using FourPrep.Common.Application.Paging;
using System.Text.RegularExpressions;
using FourPrep.Modules.Auth.Domain.Users;
using System.IdentityModel.Tokens.Jwt;
using FourPrep.Modules.Auth.Domain.Authentication;
using FourPrep.Modules.Auth.Domain.IdentityProvider;
using FourPrep.Modules.Auth.Application.Users.GetUser;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

internal sealed partial class KeycloakAdminClient(
    HttpClient httpClient,
    IOptions<KeycloakOptions> options,
    ILogger<KeycloakAdminClient> logger) : IIdentityProviderClient
{
    private const string AdminUsersUrlTemplate = "admin/realms/{0}/users";
    private const string AdminUserUrlTemplate = "admin/realms/{0}/users/{1}";
    private const string AdminUserCountUrlTemplate = "admin/realms/{0}/users/count";
    private const string AdminUserLogoutUrlTemplate = "admin/realms/{0}/users/{1}/logout";
    private const string AdminUserPasswordUrlTemplate = "admin/realms/{0}/users/{1}/reset-password";
    private const string AdminUserRealmRolesUrlTemplate = "admin/realms/{0}/users/{1}/role-mappings/realm";
    private const string AdminRoleMembersUrlTemplate = "admin/realms/{0}/roles/{1}/users";
    private const string AdminRealmRoleUrlTemplate = "admin/realms/{0}/roles/{1}";
    private const string TokenUrlTemplate = "realms/{0}/protocol/openid-connect/token";
    private const string LogoutUrlTemplate = "realms/{0}/protocol/openid-connect/logout";

    private const string JsonMediaType = "application/json";
    private const string BearerScheme = "Bearer";
    private const string DefaultTokenType = "Bearer";

    private const string GrantTypeField = "grant_type";
    private const string ClientIdField = "client_id";
    private const string ClientSecretField = "client_secret";
    private const string UsernameField = "username";
    private const string PasswordField = "password";
    private const string RefreshTokenField = "refresh_token";

    private const string PasswordGrant = "password";
    private const string ClientCredentialsGrant = "client_credentials";
    private const string RefreshTokenGrant = "refresh_token";

    private const string AccessTokenField = "access_token";
    private const string IdTokenField = "id_token";
    private const string TokenTypeField = "token_type";
    private const string ExpiresInField = "expires_in";
    private const string RefreshExpiresInField = "refresh_expires_in";

    private const string PasswordCredentialType = "password";
    private const string PhoneNumberAttribute = "phone_number";
    private const string ConflictKeyword = "exists";
    private const string ErrorMessageField = "errorMessage";
    private const string ErrorFieldName = "field";
    private const string UsernameConflictMessage = "User exists with same username";
    private const string EmailConflictMessage = "User exists with same email";

    private const string IdField = "id";
    private const string NameField = "name";
    private const string EmailField = "email";
    private const string EmailVerifiedField = "emailVerified";
    private const string EnabledField = "enabled";
    private const string AttributesField = "attributes";
    private const string CreatedTimestampField = "createdTimestamp";

    private const string SearchParameter = "search";
    private const string AttributeQueryParameter = "q";
    private const string EnabledParameter = "enabled";
    private const string FirstParameter = "first";
    private const string MaxParameter = "max";

    private const int ListingPageSize = 500;

    private const string DefaultRolesPrefix = "default-roles-";
    private static readonly string[] BuiltInRealmRoles = ["offline_access", "uma_authorization"];

    private readonly KeycloakOptions _options = options.Value;

    public async Task<Result<string>> CreateUserAsync(
        string username,
        string email,
        string? phoneNumber,
        bool emailVerified,
        bool isActive,
        string? password = null,
        CancellationToken cancellationToken = default)
    {
        using var request = await BuildAdminRequestAsync(HttpMethod.Post, BuildUsersUrl(), cancellationToken);
        request.Content = CreateJsonContent(new
        {
            username,
            email,
            enabled = isActive,
            emailVerified,
            credentials = BuildCredentials(password),
            attributes = BuildAttributes(phoneNumber),
            requiredActions = Array.Empty<string>()
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<string>(await MapErrorAsync(response, cancellationToken));
        }

        string? keycloakUserId = ExtractUserIdFromLocation(response.Headers.Location);
        return string.IsNullOrWhiteSpace(keycloakUserId)
            ? Result.Failure<string>(IdentityProviderErrors.OperationFailed)
            : keycloakUserId;
    }

    public async Task<Result> UpdateUserAsync(
        string keycloakUserId,
        string username,
        string email,
        string? phoneNumber,
        bool emailVerified,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        using var request = await BuildAdminRequestAsync(HttpMethod.Put, BuildUserUrl(keycloakUserId), cancellationToken);
        request.Content = CreateJsonContent(new
        {
            username,
            email,
            enabled = isActive,
            emailVerified,
            attributes = BuildAttributes(phoneNumber)
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(await MapErrorAsync(response, cancellationToken));
    }

    public async Task<Result> DeleteUserAsync(
        string keycloakUserId,
        CancellationToken cancellationToken = default)
    {
        using var request = await BuildAdminRequestAsync(HttpMethod.Delete, BuildUserUrl(keycloakUserId), cancellationToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(await MapErrorAsync(response, cancellationToken));
    }

    public Task<Result<IdentityTokenResult>> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, string>
        {
            [GrantTypeField] = PasswordGrant,
            [ClientIdField] = _options.ClientId,
            [ClientSecretField] = _options.ClientSecret,
            [UsernameField] = username,
            [PasswordField] = password
        };

        return RequestTokenAsync(payload, AuthenticationErrors.InvalidCredentials, cancellationToken);
    }

    public Task<Result<IdentityTokenResult>> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, string>
        {
            [GrantTypeField] = RefreshTokenGrant,
            [ClientIdField] = _options.ClientId,
            [ClientSecretField] = _options.ClientSecret,
            [RefreshTokenField] = refreshToken
        };

        return RequestTokenAsync(payload, AuthenticationErrors.InvalidRefreshToken, cancellationToken);
    }

    public async Task<Result> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildLogoutUrl())
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [ClientIdField] = _options.ClientId,
                [ClientSecretField] = _options.ClientSecret,
                [RefreshTokenField] = refreshToken
            })
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(AuthenticationErrors.LogoutFailed);
    }

    public async Task<Result<UserResponse>> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await GetAdminJsonAsync<JObject>(BuildUserUrl(userId.ToString()), cancellationToken);

        return result.IsSuccess
            ? ToUserResponse(result.Data)
            : Result.Failure<UserResponse>(ToUserError(result.Error));
    }

    public async Task<Result<PagedList<UserResponse>>> SearchUsersAsync(
        string? search,
        bool? isEnabled,
        IPagingRequest paging,
        CancellationToken cancellationToken = default)
    {
        List<string> filter = BuildUserFilter(search, isEnabled);

        string usersUrl = AppendQuery(
            BuildUsersUrl(),
            [.. filter, $"{FirstParameter}={paging.First}", $"{MaxParameter}={paging.Rows}"]);

        var users = await GetAdminJsonAsync<JArray>(usersUrl, cancellationToken);
        if (users.IsFailure)
        {
            return Result.Failure<PagedList<UserResponse>>(users.Error);
        }

        var count = await GetAdminJsonAsync<JValue>(AppendQuery(BuildUserCountUrl(), filter), cancellationToken);
        if (count.IsFailure)
        {
            return Result.Failure<PagedList<UserResponse>>(count.Error);
        }

        return new PagedList<UserResponse>(
            users.Data.OfType<JObject>().Select(ToUserResponse).ToList(),
            count.Data.Value<int>());
    }

    public async Task<Result<IReadOnlyList<UserResponse>>> GetRealmRoleMembersAsync(
        string roleName,
        CancellationToken cancellationToken = default)
    {
        var members = await ReadAllPagesAsync(BuildRoleMembersUrl(roleName), cancellationToken);

        if (members.IsSuccess)
        {
            return Result.Success<IReadOnlyList<UserResponse>>([.. members.Data.Select(ToUserResponse)]);
        }

        return members.Error == IdentityProviderErrors.UserNotFound
            ? Result.Success<IReadOnlyList<UserResponse>>([])
            : Result.Failure<IReadOnlyList<UserResponse>>(members.Error);
    }

    public async Task<Result<IReadOnlyList<string>>> GetUserRealmRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var roles = await GetAdminJsonAsync<JArray>(BuildUserRealmRolesUrl(userId), cancellationToken);
        if (roles.IsFailure)
        {
            return Result.Failure<IReadOnlyList<string>>(ToUserError(roles.Error));
        }

        IReadOnlyList<string> roleNames = [.. roles.Data
            .Select(role => role.Value<string>(NameField))
            .OfType<string>()
            .Where(name => !name.StartsWith(DefaultRolesPrefix, StringComparison.Ordinal))
            .Where(name => !BuiltInRealmRoles.Contains(name))
            .Order(StringComparer.Ordinal)];

        return Result.Success(roleNames);
    }

    public async Task<Result> AssignRealmRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        var role = await GetAdminJsonAsync<JObject>(BuildRealmRoleUrl(roleName), cancellationToken);
        if (role.IsFailure)
        {
            return Result.Failure(role.Error == IdentityProviderErrors.UserNotFound
                ? IdentityProviderErrors.RoleNotFound
                : role.Error);
        }

        using var request = await BuildAdminRequestAsync(HttpMethod.Post, BuildUserRealmRolesUrl(userId), cancellationToken);
        request.Content = CreateJsonContent(new[] { role.Data });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(ToUserError(await MapErrorAsync(response, cancellationToken)));
    }

    public async Task<Result> SetUserEnabledAsync(
        Guid userId,
        bool isEnabled,
        CancellationToken cancellationToken = default)
    {
        using var request = await BuildAdminRequestAsync(HttpMethod.Put, BuildUserUrl(userId.ToString()), cancellationToken);
        request.Content = CreateJsonContent(new Dictionary<string, bool> { [EnabledField] = isEnabled });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(ToUserError(await MapErrorAsync(response, cancellationToken)));
    }

    public async Task<Result> LogoutUserSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var request = await BuildAdminRequestAsync(HttpMethod.Post, BuildUserLogoutUrl(userId), cancellationToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(ToUserError(await MapErrorAsync(response, cancellationToken)));
    }

    public async Task<Result> SetUserPasswordAsync(
        Guid userId,
        string password,
        CancellationToken cancellationToken = default)
    {
        using var request = await BuildAdminRequestAsync(HttpMethod.Put, BuildUserPasswordUrl(userId), cancellationToken);
        request.Content = CreateJsonContent(BuildPasswordCredential(password));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(ToUserError(await MapErrorAsync(response, cancellationToken)));
    }

    private async Task<Result<TToken>> GetAdminJsonAsync<TToken>(string url, CancellationToken cancellationToken)
        where TToken : JToken
    {
        using var request = await BuildAdminRequestAsync(HttpMethod.Get, url, cancellationToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<TToken>(await MapErrorAsync(response, cancellationToken));
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JToken.Parse(content) is TToken token
            ? token
            : Result.Failure<TToken>(IdentityProviderErrors.OperationFailed);
    }

    private async Task<Result<List<JObject>>> ReadAllPagesAsync(string url, CancellationToken cancellationToken)
    {
        var items = new List<JObject>();
        string separator = url.Contains('?') ? "&" : "?";

        for (int first = 0; ; first += ListingPageSize)
        {
            string pageUrl = $"{url}{separator}{FirstParameter}={first}&{MaxParameter}={ListingPageSize}";

            var page = await GetAdminJsonAsync<JArray>(pageUrl, cancellationToken);
            if (page.IsFailure)
            {
                return Result.Failure<List<JObject>>(page.Error);
            }

            items.AddRange(page.Data.OfType<JObject>());

            if (page.Data.Count < ListingPageSize)
            {
                return items;
            }
        }
    }

    private async Task<HttpRequestMessage> BuildAdminRequestAsync(
        HttpMethod method,
        string url,
        CancellationToken cancellationToken)
    {
        string accessToken = await GetAdminAccessTokenAsync(cancellationToken);
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, accessToken);
        return request;
    }

    private async Task<string> GetAdminAccessTokenAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildTokenUrl())
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [GrantTypeField] = ClientCredentialsGrant,
                [ClientIdField] = _options.ClientId,
                [ClientSecretField] = _options.ClientSecret
            })
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        string payload = await response.Content.ReadAsStringAsync(cancellationToken);
        string? accessToken = JObject.Parse(payload)[AccessTokenField]?.Value<string>();

        return string.IsNullOrWhiteSpace(accessToken)
            ? throw new ApplicationException("Keycloak access token is unavailable.")
            : accessToken;
    }

    private async Task<Result<IdentityTokenResult>> RequestTokenAsync(
        Dictionary<string, string> payload,
        Error failureError,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildTokenUrl())
        {
            Content = new FormUrlEncodedContent(payload)
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return await MapTokenFailureAsync(response, failureError, cancellationToken);
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        JObject json = JObject.Parse(content);

        string? accessToken = json[AccessTokenField]?.Value<string>();
        string? refreshToken = json[RefreshTokenField]?.Value<string>();

        if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Failure<IdentityTokenResult>(failureError);
        }

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        if (!Guid.TryParse(jwt.Subject, out Guid userId))
        {
            return Result.Failure<IdentityTokenResult>(failureError);
        }

        return new IdentityTokenResult(
            accessToken,
            refreshToken,
            json[IdTokenField]?.Value<string>(),
            json[TokenTypeField]?.Value<string>() ?? DefaultTokenType,
            json[ExpiresInField]?.Value<int>() ?? 0,
            json[RefreshExpiresInField]?.Value<int>() ?? 0,
            userId,
            jwt.ValidTo);
    }

    private async Task<Result<IdentityTokenResult>> MapTokenFailureAsync(
        HttpResponseMessage response,
        Error failureError,
        CancellationToken cancellationToken)
    {
        string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

        logger.LogWarning(
            "Keycloak rejected the token request with {StatusCode}: {ErrorContent}",
            (int)response.StatusCode,
            errorContent);

        return Result.Failure<IdentityTokenResult>(failureError);
    }

    private string BuildUsersUrl() =>
        string.Format(AdminUsersUrlTemplate, _options.Realm);

    private string BuildUserUrl(string keycloakUserId) =>
        string.Format(AdminUserUrlTemplate, _options.Realm, keycloakUserId);

    private string BuildUserCountUrl() =>
        string.Format(AdminUserCountUrlTemplate, _options.Realm);

    private string BuildUserLogoutUrl(Guid userId) =>
        string.Format(AdminUserLogoutUrlTemplate, _options.Realm, userId);

    private string BuildUserPasswordUrl(Guid userId) =>
        string.Format(AdminUserPasswordUrlTemplate, _options.Realm, userId);

    private string BuildUserRealmRolesUrl(Guid userId) =>
        string.Format(AdminUserRealmRolesUrlTemplate, _options.Realm, userId);

    private string BuildRoleMembersUrl(string roleName) =>
        string.Format(AdminRoleMembersUrlTemplate, _options.Realm, Uri.EscapeDataString(roleName));

    private string BuildRealmRoleUrl(string roleName) =>
        string.Format(AdminRealmRoleUrlTemplate, _options.Realm, Uri.EscapeDataString(roleName));

    private string BuildTokenUrl() =>
        string.Format(TokenUrlTemplate, _options.Realm);

    private string BuildLogoutUrl() =>
        string.Format(LogoutUrlTemplate, _options.Realm);

    private static StringContent CreateJsonContent(object payload) =>
        new(JsonConvert.SerializeObject(payload), Encoding.UTF8, JsonMediaType);

    private static object BuildAttributes(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return new { };
        }

        return new Dictionary<string, string[]>
        {
            [PhoneNumberAttribute] = [phoneNumber]
        };
    }

    private static object[]? BuildCredentials(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        return [BuildPasswordCredential(password)];
    }

    private static object BuildPasswordCredential(string password) =>
        new
        {
            type = PasswordCredentialType,
            value = password,
            temporary = false
        };

    private static List<string> BuildUserFilter(string? search, bool? isEnabled)
    {
        var parameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();

            parameters.Add(PhoneNumberPattern().IsMatch(term)
                ? $"{AttributeQueryParameter}={Uri.EscapeDataString($"{PhoneNumberAttribute}:{term}")}"
                : $"{SearchParameter}={Uri.EscapeDataString(term)}");
        }

        if (isEnabled is not null)
        {
            parameters.Add($"{EnabledParameter}={(isEnabled.Value ? "true" : "false")}");
        }

        return parameters;
    }

    private static string AppendQuery(string url, IReadOnlyCollection<string> parameters) =>
        parameters.Count == 0 ? url : $"{url}?{string.Join('&', parameters)}";

    private static UserResponse ToUserResponse(JObject json) =>
        new(
            Guid.Parse(json.Value<string>(IdField)!),
            json.Value<string>(UsernameField) ?? string.Empty,
            json.Value<string>(EmailField),
            json.Value<bool?>(EmailVerifiedField) ?? false,
            json[AttributesField]?[PhoneNumberAttribute]?.First?.Value<string>(),
            json.Value<bool?>(EnabledField) ?? false,
            ReadCreatedOnUtc(json));

    private static DateTimeOffset? ReadCreatedOnUtc(JObject json)
    {
        long? createdTimestamp = json.Value<long?>(CreatedTimestampField);

        return createdTimestamp is null
            ? null
            : DateTimeOffset.FromUnixTimeMilliseconds(createdTimestamp.Value);
    }

    private static Error ToUserError(Error error) =>
        error == IdentityProviderErrors.UserNotFound ? UserErrors.NotFound : error;

    [GeneratedRegex(@"^\+?\d{7,15}$")]
    private static partial Regex PhoneNumberPattern();

    private static string? ExtractUserIdFromLocation(Uri? location) =>
        location?.Segments.LastOrDefault()?.TrimEnd('/');

    private async Task<Error> MapErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return IdentityProviderErrors.UserNotFound;
        }

        JObject? errorBody = ReadErrorBody(content);

        if (response.StatusCode == HttpStatusCode.Conflict ||
            content.Contains(ConflictKeyword, StringComparison.OrdinalIgnoreCase))
        {
            return ToConflictError(errorBody);
        }

        if (response.StatusCode == HttpStatusCode.BadRequest &&
            errorBody?.Value<string>(ErrorFieldName) == UsernameField)
        {
            return UserErrors.InvalidUsername;
        }

        logger.LogWarning(
            "Keycloak admin request failed with {StatusCode}: {ErrorContent}",
            (int)response.StatusCode,
            content);

        return IdentityProviderErrors.OperationFailed;
    }

    private static Error ToConflictError(JObject? errorBody) =>
        errorBody?.Value<string>(ErrorMessageField) switch
        {
            UsernameConflictMessage => UserErrors.UsernameTaken,
            EmailConflictMessage => UserErrors.EmailTaken,
            _ => IdentityProviderErrors.Conflict
        };

    private static JObject? ReadErrorBody(string content)
    {
        try
        {
            return JToken.Parse(content) as JObject;
        }
        catch (JsonReaderException)
        {
            return null;
        }
    }
}
