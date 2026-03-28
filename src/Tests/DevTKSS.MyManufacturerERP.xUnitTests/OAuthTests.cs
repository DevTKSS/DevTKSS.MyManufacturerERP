using DevTKSS.Extensions.OAuth;
using DevTKSS.Extensions.OAuth.Defaults;
using DevTKSS.Extensions.OAuth.Dictionarys;
using DevTKSS.Extensions.OAuth.Options;
using DevTKSS.Extensions.OAuth.Requests;
using DevTKSS.Extensions.OAuth.Utils;
using DevTKSS.Extensions.OAuth.Validation;
using FluentValidation;

namespace DevTKSS.MyManufacturerERP.xUnitTests;

public class OAuthTests
{
    #region AuthDictionaryExtensions

    [Fact]
    public void TryGetCode_ShouldReturnCode()
    {
        var dict = new Dictionary<string, string>
        {
            ["code"] = "abc123"
        };

        dict.TryGetCode(out var code).ShouldBeTrue();
        code.ShouldBe("abc123");
    }

    [Fact]
    public void TryGetCode_ShouldReturnFalse_WhenNoCodeKey()
    {
        var dict = new Dictionary<string, string>
        {
            ["other"] = "value"
        };

        dict.TryGetCode(out var code).ShouldBeFalse();
        code.ShouldBeNull();
    }

#pragma warning disable CS0618 // Type or member is obsolete
    [Fact]
    public void TryGetAuthorizationCode_ShouldDelegateToTryGetCode()
    {
        var dict = new Dictionary<string, string>
        {
            [OAuthDefaults.Keys.Code] = "test_code_123"
        };

        dict.TryGetAuthorizationCode(out var code).ShouldBeTrue();
        code.ShouldBe("test_code_123");
    }

    [Fact]
    public void TryGetAuthorizationCode_ShouldNotMatchGrantTypeValue()
    {
        var dict = new Dictionary<string, string>
        {
            [OAuthDefaults.Values.AuthorizationCode] = "should_not_match"
        };

        dict.TryGetAuthorizationCode(out _).ShouldBeFalse();
    }
#pragma warning restore CS0618

    [Fact]
    public void TryGetErrorCode_ShouldDetectError()
    {
        var dict = new Dictionary<string, string>
        {
            ["error"] = "access_denied"
        };

        dict.TryGetErrorCode(out var error).ShouldBeTrue();
        error.ShouldBe("access_denied");
    }

    [Fact]
    public void TryGetState_ShouldReturnState()
    {
        var dict = new Dictionary<string, string>
        {
            ["state"] = "random_state_value"
        };

        dict.TryGetState(out var state).ShouldBeTrue();
        state.ShouldBe("random_state_value");
    }

    [Fact]
    public void TryGetCodeVerifier_ShouldReturnVerifier()
    {
        var dict = new Dictionary<string, string>
        {
            [OAuthDefaults.Keys.Pkce.CodeVerifier] = "verifier_value"
        };

        dict.TryGetCodeVerifier(out var verifier).ShouldBeTrue();
        verifier.ShouldBe("verifier_value");
    }

    [Fact]
    public void IsErrorResponse_ShouldReturnTrue_WhenErrorPresent()
    {
        var dict = new Dictionary<string, string>
        {
            ["error"] = "invalid_grant"
        };

        dict.IsErrorResponse().ShouldBeTrue();
    }

    [Fact]
    public void IsErrorResponse_ShouldReturnFalse_WhenNoError()
    {
        var dict = new Dictionary<string, string>
        {
            ["code"] = "abc"
        };

        dict.IsErrorResponse().ShouldBeFalse();
    }

    #endregion

    #region AuthorizationState

    [Fact]
    public void AuthorizationState_ShouldGenerateAllValues()
    {
        var state = new AuthorizationState();

        state.State.ShouldNotBeNullOrWhiteSpace();
        state.CodeVerifier.ShouldNotBeNullOrWhiteSpace();
        state.CodeChallenge.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void AuthorizationState_ShouldGenerateUniqueValues()
    {
        var state1 = new AuthorizationState();
        var state2 = new AuthorizationState();

        state1.State.ShouldNotBe(state2.State);
        state1.CodeVerifier.ShouldNotBe(state2.CodeVerifier);
    }

    #endregion

    #region PKCE Utilities

    [Fact]
    public void GenerateCodeVerifier_ShouldHaveCorrectLength()
    {
        var verifier = OAuth2Utilitys.GenerateCodeVerifier();
        verifier.Length.ShouldBeGreaterThanOrEqualTo(43);
        verifier.Length.ShouldBeLessThanOrEqualTo(128);
    }

    [Fact]
    public void GenerateCodeChallenge_ShouldBeDeterministic()
    {
        var verifier = OAuth2Utilitys.GenerateCodeVerifier();
        var challenge1 = OAuth2Utilitys.GenerateCodeChallenge(verifier);
        var challenge2 = OAuth2Utilitys.GenerateCodeChallenge(verifier);

        challenge1.ShouldBe(challenge2);
    }

    [Fact]
    public void GenerateState_ShouldBeUnique()
    {
        var state1 = OAuth2Utilitys.GenerateState();
        var state2 = OAuth2Utilitys.GenerateState();

        state1.ShouldNotBe(state2);
    }

    #endregion

    #region OAuthClientOptions Validation

    [Fact]
    public void Validator_ShouldPass_WhenPkceAndNoSecret()
    {
        var options = CreateValidOptions(usePkce: true);
        options.ClientSecret = null;

        var validator = new OAuthClientOptionsValidator();
        var result = validator.Validate(options);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validator_ShouldFail_WhenNoPkceAndNoSecret()
    {
        var options = CreateValidOptions(usePkce: false);
        options.ClientSecret = null;

        var validator = new OAuthClientOptionsValidator();
        var result = validator.Validate(options);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "ClientSecret");
    }

    [Fact]
    public void Validator_ShouldFail_WhenClientIdEmpty()
    {
        var options = CreateValidOptions();
        options.ClientId = "";

        var validator = new OAuthClientOptionsValidator();
        var result = validator.Validate(options);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "ClientId");
    }

    [Fact]
    public void Validator_ShouldFail_WhenTokenEndpointInvalid()
    {
        var options = CreateValidOptions();
        options.TokenEndpoint = "not-a-url";

        var validator = new OAuthClientOptionsValidator();
        var result = validator.Validate(options);

        result.IsValid.ShouldBeFalse();
    }

    private static OAuthClientOptions CreateValidOptions(bool usePkce = true) => new()
    {
        AuthorizationEndpoint = "https://www.etsy.com/oauth/connect",
        TokenEndpoint = "https://api.etsy.com/v3/public/oauth/token",
        ClientId = "test-client-id",
        ClientSecret = "test-secret",
        RedirectUri = "http://localhost:5001/auth/callback",
        Scopes = ["shops_r", "email_r"],
        UsePkce = usePkce,
    };

    #endregion

    #region OAuthClientOptionsExtensions

    [Fact]
    public void BuildAuthorizationStartUrl_ShouldContainAllParameters()
    {
        var options = CreateValidOptions();
        var state = new AuthorizationState();

        var url = options.BuildAuthorizationStartUrl(state);

        url.ShouldContain("client_id=test-client-id");
        url.ShouldContain("redirect_uri=");
        url.ShouldContain("scope=");
        url.ShouldContain("state=");
        url.ShouldContain("code_challenge=");
        url.ShouldContain("response_type=code");
    }

    [Fact]
    public void ToWebAuthRequest_ShouldSetCallbackUrl()
    {
        var options = CreateValidOptions();
        var state = new AuthorizationState();

        var request = options.ToWebAuthRequest(state);

        request.CallbackUrl.ShouldBe("http://localhost:5001/auth/callback");
        request.StartUrl.ShouldStartWith("https://www.etsy.com/oauth/connect?");
    }

    #endregion
}
