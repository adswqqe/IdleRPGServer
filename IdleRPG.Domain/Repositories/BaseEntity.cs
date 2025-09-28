namespace IdleRPG.Domain.Repositories
{
// Unity의 MonoBehaviour처럼 기본 베이스 클래스
    public abstract class BaseEntity<TKey>
    {
        public TKey Id { get; set; }
    }
}