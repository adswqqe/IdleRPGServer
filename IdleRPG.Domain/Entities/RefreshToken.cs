namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// Refresh Token 엔티티 (DB 저장용)
    /// Access Token은 짧게, Refresh Token은 길게!
    /// </summary>
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; }
        public Guid PlayerId { get; set; }
        public Player Player { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        
        /// <summary>
        /// 토큰이 유효한지 확인 (Unity의 IsValid 메서드처럼)
        /// </summary>
        public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;
    }
}