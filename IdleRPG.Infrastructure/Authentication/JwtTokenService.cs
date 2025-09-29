using IdleRPG.Application.Services;
using IdleRPG.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace IdleRPG.Infrastructure.Authentication
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(IOptions<JwtSettings> jwtSettings, ILogger<JwtTokenService> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _jwtSettings.Validate();
            _logger = logger;
        }
        
        /// <summary>
        /// Access Token 생성 - Unity의 JsonUtility.ToJson과 유사한 개념
        /// </summary>
        public string GenerateAccessToken(Player player)
        {
            // 1. 보안 키 생성 (Unity의 Private Key처럼)
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            // 2. 서명 알고리즘 설정 (HMAC-SHA256)
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            // 3. 토큰에 담을 정보 (Claims) 생성
            // Unity의 직렬화 필드처럼 필요한 정보만 담기
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, player.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),

                // 커스텀 클레임 (게임 정보)
                new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
                new Claim(ClaimTypes.Name, player.UserName),
                new Claim(ClaimTypes.Email, player.Email),
                new Claim("Level", player.Stats?.Level.ToString() ?? "1"),

                // 권한 (일반 유저 / 관리자 등)
                new Claim(ClaimTypes.Role, "Player")
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            
            _logger.LogInformation(
                $"Access token generated for user {player.UserName} (ID: {player.Id})");
            
            return tokenString;
        }

        /// <summary>
        /// Refresh Token 생성 - 랜덤 문자열
        /// </summary>
        public string GenerateRefreshToken()
        {
            // 안전한 랜덤 바이트 생성
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            
            // Base64로 인코딩하여 문자열로 변환
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// 토큰에서 사용자 ID 추출 - Unity의 JsonUtility.FromJson처럼
        /// </summary>
        public Guid? GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                // "sub" 클레임에서 사용자 ID 추출
                var userIdClaim = jwtToken.Claims.FirstOrDefault(
                    c => c.Type == JwtRegisteredClaimNames.Sub);
                
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract user ID from token");
                return null;
            }
        }

        /// <summary>
        /// 토큰 유효성 검증
        /// </summary>
        public bool ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;
            
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetTokenValidationParameters();
                
                // 토큰 검증 (서명, 만료 시간 등)
                tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token validation");
                return false;
            }
        }
        
        /// <summary>
        /// 토큰 검증 파라미터 생성
        /// </summary>
        private TokenValidationParameters GetTokenValidationParameters()
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,         // 만료 시간 검증
                ValidateIssuerSigningKey = true, // 서명 검증
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = securityKey,
                ClockSkew = TimeSpan.Zero             // 시간 여유 없음 (정확히)
            };
        }
    }
}