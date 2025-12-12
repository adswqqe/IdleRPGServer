namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// 모든 Entity의 기본 베이스 클래스 (Unity의 MonoBehaviour와 유사)
    /// </summary>
    /// <typeparam name="TKey">식별자 타입</typeparam>
    public abstract class BaseEntity<TKey>
    {
        /// <summary>
        /// 고유 식별자
        /// </summary>
        public TKey Id { get; set; } = default!;

        /// <summary>
        /// 생성 시간 (UTC)
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Guid 식별자를 사용하는 Entity의 베이스 클래스
    /// </summary>
    public abstract class BaseEntity : BaseEntity<Guid>
    {
        /// <summary>
        /// 고유 식별자 (Guid, 자동 생성)
        /// </summary>
        public new Guid Id { get; set; } = Guid.NewGuid();
    }
}