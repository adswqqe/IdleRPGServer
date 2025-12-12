namespace IdleRPG.Infrastructure.Authentication
{
    /// <summary>
    /// JWT 설정을 담는 클래스 (Unity의 ScriptableObject처럼)
    /// </summary>
    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessTokenExpirationMinutes { get; set; }
        public int RefreshTokenExpirationDays { get; set; }

        /// <summary>
        /// 설정 검증
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrEmpty(SecretKey) || SecretKey.Length < 32)
            {
                throw new InvalidOperationException(
                    "JWT SecretKey must be at least 32 characters long for HS256 algorithm");
            }
            
            if (string.IsNullOrEmpty(Issuer))
                throw new InvalidOperationException("JWT Issuer is required");
                
            if (string.IsNullOrEmpty(Audience))
                throw new InvalidOperationException("JWT Audience is required");
        }
    }
}