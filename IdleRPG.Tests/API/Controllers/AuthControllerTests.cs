using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IdleRPG.Application.DTOs.Auth;
using IdleRPG.Domain.Entities;
using IdleRPG.Infrastructure.Data;
using IdleRPG.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IdleRPG.Tests.API.Controllers;

/// <summary>
/// AuthController의 통합 테스트를 수행합니다.
/// Phase 2: 보안 검증 - JWT 인증 흐름, 중복 가입 방지, 토큰 갱신 등
/// </summary>
/// <remarks>
/// 🎓 학습 목적: JWT 기반 인증/인가 흐름 이해 및 보안 취약점 테스트 방법 학습
/// ✅ Public 엔드포인트: Register, Login, Refresh (인증 불필요)
/// ⚠️ Authorized 엔드포인트: Logout, Profile (JWT 토큰 필요 - TODO(human))
/// </remarks>
public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    #region POST /api/auth/register (Public Endpoint)

    [Fact]
    public async Task Register_ShouldReturn201Created_WhenValidDataProvided()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "NewPlayer",
            Email = "newplayer@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("accessToken");
        content.Should().Contain("refreshToken");

        // JSON 파싱하여 실제 토큰 존재 확인
        var jsonDoc = System.Text.Json.JsonDocument.Parse(content);
        var authResponse = jsonDoc.RootElement.GetProperty("response");

        authResponse.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        authResponse.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
        authResponse.GetProperty("userName").GetString().Should().Be("NewPlayer");
    }

    [Fact]
    public async Task Register_ShouldReturn400BadRequest_WhenUsernameAlreadyExists()
    {
        // Arrange: 기존 사용자 먼저 등록
        var firstUser = new RegisterDto
        {
            Username = "DuplicateUser",
            Email = "duplicate1@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };
        await _client.PostAsJsonAsync("/api/auth/register", firstUser);

        // Act: 동일한 Username으로 다시 가입 시도
        var duplicateUser = new RegisterDto
        {
            Username = "DuplicateUser", // 중복!
            Email = "duplicate2@test.com", // 이메일은 다름
            Password = "Password456!",
            ConfirmPassword = "Password456!"
        };
        var response = await _client.PostAsJsonAsync("/api/auth/register", duplicateUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("message");
    }

    [Fact]
    public async Task Register_ShouldReturn400BadRequest_WhenPasswordTooShort()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "ShortPass",
            Email = "shortpass@test.com",
            Password = "123", // 6자 미만
            ConfirmPassword = "123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturn400BadRequest_WhenPasswordsDoNotMatch()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "MismatchPass",
            Email = "mismatch@test.com",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword!" // 불일치
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region POST /api/auth/login (Public Endpoint)

    [Fact]
    public async Task Login_ShouldReturn200OK_WhenCredentialsAreValid()
    {
        // Arrange: 먼저 사용자 등록
        var registerDto = new RegisterDto
        {
            Username = "LoginTestUser",
            Email = "logintest@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Act: 등록한 계정으로 로그인
        var loginDto = new LoginDto
        {
            Username = "LoginTestUser",
            Password = "Password123!"
        };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("accessToken");
        content.Should().Contain("refreshToken");

        var jsonDoc = System.Text.Json.JsonDocument.Parse(content);
        var authResponse = jsonDoc.RootElement.GetProperty("response");

        authResponse.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        authResponse.GetProperty("userName").GetString().Should().Be("LoginTestUser");
    }

    [Fact]
    public async Task Login_ShouldReturn401Unauthorized_WhenPasswordIsIncorrect()
    {
        // Arrange: 먼저 사용자 등록
        var registerDto = new RegisterDto
        {
            Username = "WrongPassUser",
            Email = "wrongpass@test.com",
            Password = "CorrectPassword123!",
            ConfirmPassword = "CorrectPassword123!"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Act: 잘못된 비밀번호로 로그인 시도
        var loginDto = new LoginDto
        {
            Username = "WrongPassUser",
            Password = "WrongPassword!" // 잘못된 비밀번호
        };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("message");
    }

    [Fact]
    public async Task Login_ShouldReturn401Unauthorized_WhenUserDoesNotExist()
    {
        // Arrange & Act: 존재하지 않는 사용자로 로그인 시도
        var loginDto = new LoginDto
        {
            Username = "NonExistentUser",
            Password = "SomePassword123!"
        };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region POST /api/auth/refresh (Public Endpoint)

    [Fact]
    public async Task RefreshToken_ShouldReturn200OK_WhenRefreshTokenIsValid()
    {
        // Arrange: 먼저 사용자 등록하여 Refresh Token 획득
        var registerDto = new RegisterDto
        {
            Username = "RefreshTestUser",
            Email = "refreshtest@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var registerJson = System.Text.Json.JsonDocument.Parse(registerContent);
        var refreshToken = registerJson.RootElement.GetProperty("response").GetProperty("refreshToken").GetString();

        // Act: Refresh Token으로 새 Access Token 발급
        var refreshDto = new RefreshTokenDto
        {
            RefreshToken = refreshToken
        };
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = System.Text.Json.JsonDocument.Parse(content);
        var authResponse = jsonDoc.RootElement.GetProperty("response");

        authResponse.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        authResponse.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_ShouldReturn401Unauthorized_WhenRefreshTokenIsInvalid()
    {
        // Arrange: 유효하지 않은 Refresh Token
        var refreshDto = new RefreshTokenDto
        {
            RefreshToken = "invalid-refresh-token-12345"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region POST /api/auth/logout (Authorized - TODO(human))

    /// <summary>
    /// 🎓 TODO(human): JWT 토큰 생성 Helper를 작성하여 인증 테스트를 완성하세요.
    /// </summary>
    /// <remarks>
    /// 현재 프로젝트의 JWT 설정:
    /// - Program.cs의 JwtOptions (Issuer, Audience, SigningKey)
    /// - System.IdentityModel.Tokens.Jwt의 JwtSecurityTokenHandler 사용
    /// - Authorization 헤더에 "Bearer {token}" 추가
    ///
    /// 구현 가이드:
    /// 1. IdleRPG.Tests/Helpers/JwtTokenHelper.cs 생성
    /// 2. GenerateTestToken(Guid userId, string userName) 메서드 작성
    ///    - JwtSecurityTokenHandler로 토큰 생성
    ///    - Claims: NameIdentifier (userId), Name (userName)
    ///    - Program.cs의 JwtOptions와 동일한 설정 사용
    /// 3. _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token)
    ///
    /// 참고:
    /// - Microsoft.IdentityModel.Tokens의 SigningCredentials
    /// - System.Security.Claims의 ClaimsIdentity
    /// </remarks>
    [Fact]
    public async Task Logout_ShouldReturn200OK_WhenAuthenticatedUserLogsOut()
    {
        // Arrange: 사용자 등록 후 Refresh Token 획득
        var registerDto = new RegisterDto
        {
            Username = "LogoutTestUser",
            Email = "logouttest@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var registerJson = System.Text.Json.JsonDocument.Parse(registerContent);
        var refreshToken = registerJson.RootElement.GetProperty("response").GetProperty("refreshToken").GetString();
        var userId = registerJson.RootElement.GetProperty("response").GetProperty("playerId").GetString();

        // JWT 토큰 생성하여 Authorization 헤더에 추가
        var token = JwtTokenHelper.GenerateTestToken(Guid.Parse(userId!), "LogoutTestUser");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act: 로그아웃
        var logoutDto = new RefreshTokenDto { RefreshToken = refreshToken! };
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        // Arrange: Authorization 헤더 없이 요청
        var logoutDto = new RefreshTokenDto { RefreshToken = "some-token" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /api/auth/profile (Authorized - TODO(human))

    [Fact]
    public async Task GetProfile_ShouldReturn200OK_WhenAuthenticated()
    {
        // Arrange: 사용자 등록
        var registerDto = new RegisterDto
        {
            Username = "ProfileTestUser",
            Email = "profiletest@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var registerJson = System.Text.Json.JsonDocument.Parse(registerContent);
        var userId = registerJson.RootElement.GetProperty("response").GetProperty("playerId").GetString();

        // JWT 토큰 생성하여 Authorization 헤더에 추가
        var token = JwtTokenHelper.GenerateTestToken(Guid.Parse(userId!), "ProfileTestUser");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/auth/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("ProfileTestUser");
    }

    [Fact]
    public async Task GetProfile_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        // Arrange: Authorization 헤더 없이 요청
        // Act
        var response = await _client.GetAsync("/api/auth/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region 보안 검증 테스트

    [Fact]
    public async Task Register_ShouldNotAllowSqlInjection_InUsername()
    {
        // Arrange: SQL Injection 시도
        var registerDto = new RegisterDto
        {
            Username = "'; DROP TABLE Players; --",
            Email = "sqlinjection@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert: Validation 실패 (Username에 특수문자 불가)
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldNotLeakUserExistence_ThroughDifferentErrorMessages()
    {
        // Arrange: 존재하지 않는 사용자와 잘못된 비밀번호의 에러 메시지가 동일해야 함 (보안)
        var nonExistentUserDto = new LoginDto
        {
            Username = "NonExistentUser123",
            Password = "Password123!"
        };

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/auth/login", nonExistentUserDto);
        var content1 = await response1.Content.ReadAsStringAsync();

        // Arrange: 실제 사용자 등록 후 잘못된 비밀번호 시도
        var registerDto = new RegisterDto
        {
            Username = "ExistingUser",
            Email = "existing@test.com",
            Password = "CorrectPassword123!",
            ConfirmPassword = "CorrectPassword123!"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var wrongPasswordDto = new LoginDto
        {
            Username = "ExistingUser",
            Password = "WrongPassword123!"
        };
        var response2 = await _client.PostAsJsonAsync("/api/auth/login", wrongPasswordDto);
        var content2 = await response2.Content.ReadAsStringAsync();

        // Assert: 두 에러 메시지가 유사해야 함 (사용자 존재 여부 노출 방지)
        response1.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response2.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // 에러 메시지 구조 확인 (정확한 메시지는 구현에 따라 다를 수 있음)
        content1.Should().Contain("message");
        content2.Should().Contain("message");
    }

    #endregion
}
