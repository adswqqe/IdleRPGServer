using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace IdleRPG.Tests.Helpers;

/// <summary>
/// 테스트용 JWT 토큰 생성 Helper 클래스
/// 실제 인증 흐름을 테스트하기 위해 유효한 JWT 토큰을 생성합니다.
/// </summary>
/// <remarks>
/// 🎓 학습 목표:
/// - JWT 토큰의 구조 (Header, Payload, Signature) 이해
/// - Claims 기반 인증 메커니즘 이해
/// - SymmetricSecurityKey를 사용한 토큰 서명 방법 학습
/// </remarks>
public static class JwtTokenHelper
{
    // Program.cs의 JwtSettings와 동일한 설정 사용
    private const string SecretKey = "IdleRPG-Super-Secret-Key-Min-32-Characters-For-HS256-Algorithm";
    private const string Issuer = "IdleRPGServer";
    private const string Audience = "IdleRPGClient";

    /// <summary>
    /// 테스트용 JWT Access Token을 생성합니다.
    /// </summary>
    /// <param name="userId">사용자 ID (Player.Id)</param>
    /// <param name="userName">사용자 이름 (Player.UserName)</param>
    /// <returns>JWT Access Token 문자열 (Bearer 토큰으로 사용)</returns>
    /// <remarks>
    /// 구현 완료: JWT 토큰 생성 로직
    /// - Claims: NameIdentifier, Name, Jti
    /// - 알고리즘: HS256 (HmacSha256)
    /// - 유효 기간: 1시간 (테스트용)
    /// - UTC 시간 기준
    /// </remarks>
    public static string GenerateTestToken(Guid userId, string userName)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(ClaimTypes.Name, userName), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(Issuer, Audience, claims, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
