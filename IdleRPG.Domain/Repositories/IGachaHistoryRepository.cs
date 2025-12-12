using IdleRPG.Domain.Entities;

namespace IdleRPG.Domain.Repositories
{
    /// <summary>
    /// Repository interface for GachaHistory entity.
    /// Handles gacha history tracking and analytics.
    /// </summary>
    public interface IGachaHistoryRepository
    {
        /// <summary>
        /// Gets a gacha history record by its ID.
        /// </summary>
        /// <param name="id">The gacha history ID.</param>
        /// <returns>The gacha history record if found; otherwise, null.</returns>
        Task<GachaHistory?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all gacha history for a specific character.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <returns>A list of gacha history records for the character.</returns>
        Task<List<GachaHistory>> GetByCharacterIdAsync(Guid characterId);

        /// <summary>
        /// Gets the most recent gacha records for a character.
        /// Useful for displaying recent pulls.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <param name="count">The number of recent records to retrieve.</param>
        /// <returns>A list of the most recent gacha history records.</returns>
        Task<List<GachaHistory>> GetRecentByCharacterIdAsync(Guid characterId, int count);

        /// <summary>
        /// Gets all gacha history records for a character within a date range.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>A list of gacha history records within the date range.</returns>
        Task<List<GachaHistory>> GetByCharacterIdAndDateRangeAsync(
            Guid characterId, 
            DateTime startDate, 
            DateTime endDate);

        /// <summary>
        /// Counts the total number of gacha pulls for a character.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <returns>The total count of gacha pulls.</returns>
        Task<int> CountByCharacterIdAsync(Guid characterId);

        /// <summary>
        /// Counts the number of pity-triggered gacha pulls for a character.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <returns>The count of pity pulls.</returns>
        Task<int> CountPityPullsByCharacterIdAsync(Guid characterId);

        /// <summary>
        /// Deletes a gacha history record.
        /// </summary>
        /// <param name="gachaHistory">The gacha history record to delete.</param>
        void Delete(GachaHistory gachaHistory);
    }
}
