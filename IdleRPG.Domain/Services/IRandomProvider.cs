namespace IdleRPG.Domain.Services
{
    /// <summary>
    /// 난수 생성을 추상화한 인터페이스 (테스트 가능성을 위해)
    /// </summary>
    public interface IRandomProvider
    {
        /// <summary>
        /// 0부터 maxValue-1까지의 난수를 반환합니다.
        /// </summary>
        /// <param name="maxValue">최대값 (exclusive)</param>
        /// <returns>생성된 난수</returns>
        int Next(int maxValue);
    }
}
