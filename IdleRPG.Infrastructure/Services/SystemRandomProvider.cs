using IdleRPG.Domain.Services;

namespace IdleRPG.Infrastructure.Services
{
    /// <summary>
    /// System.Random.Shared를 사용하는 IRandomProvider 구현체
    /// </summary>
    public class SystemRandomProvider : IRandomProvider
    {
        public int Next(int maxValue)
        {
            return Random.Shared.Next(maxValue);
        }
    }
}
