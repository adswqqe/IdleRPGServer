using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// 펫 리포지토리 인터페이스
    /// </summary>
    public interface IPetRepository
    {
        /// <summary>
        /// ID로 펫을 조회합니다.
        /// </summary>
        /// <param name="petId">펫 ID</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>펫 엔티티 (없으면 null)</returns>
        Task<Pet?> GetByIdAsync(int petId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 캐릭터가 소유한 모든 펫을 조회합니다.
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>펫 목록</returns>
        Task<List<Pet>> GetByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 캐릭터가 이미 소유한 중복 펫을 조회합니다 (가챠 중복 체크용).
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="templateId">펫 템플릿 ID</param>
        /// <param name="cancellationToken">취소 토큰</param>
        /// <returns>중복 펫 (없으면 null)</returns>
        Task<Pet?> GetDuplicateAsync(Guid characterId, int templateId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 펫을 추가합니다.
        /// </summary>
        /// <param name="pet">펫 엔티티</param>
        /// <param name="cancellationToken">취소 토큰</param>
        Task AddAsync(Pet pet, CancellationToken cancellationToken = default);

        /// <summary>
        /// 펫을 업데이트합니다.
        /// </summary>
        /// <param name="pet">펫 엔티티</param>
        /// <param name="cancellationToken">취소 토큰</param>
        Task UpdateAsync(Pet pet, CancellationToken cancellationToken = default);

        /// <summary>
        /// 펫을 삭제합니다.
        /// </summary>
        /// <param name="pet">펫 엔티티</param>
        /// <param name="cancellationToken">취소 토큰</param>
        Task DeleteAsync(Pet pet, CancellationToken cancellationToken = default);
    }
}
