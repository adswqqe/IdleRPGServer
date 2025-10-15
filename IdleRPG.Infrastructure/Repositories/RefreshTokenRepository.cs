using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Repositories
{
    /// <summary>
    /// Refresh Token Repository 구현
    /// </summary>
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly GameDBContext _context;

        public RefreshTokenRepository(GameDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Token 문자열로 RefreshToken을 조회 (Player 포함)
        /// </summary>
        public async Task<RefreshToken?> GetByTokenWithPlayerAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.Player)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        /// <summary>
        /// PlayerId와 Token으로 RefreshToken을 조회
        /// </summary>
        public async Task<RefreshToken?> GetByPlayerIdAndTokenAsync(Guid playerId, string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.PlayerId == playerId && rt.Token == token);
        }

        // IRepository<RefreshToken> 기본 메서드 구현
        public async Task<IEnumerable<RefreshToken>> GetAllAsync()
        {
            return await _context.RefreshTokens.ToListAsync();
        }

        public async Task<RefreshToken> AddAsync(RefreshToken entity)
        {
            await _context.RefreshTokens.AddAsync(entity);
            return entity;
        }

        public async Task UpdateAsync(RefreshToken entity)
        {
            _context.RefreshTokens.Update(entity);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<RefreshToken>> FindAsync(System.Linq.Expressions.Expression<Func<RefreshToken, bool>> predicate)
        {
            return await _context.RefreshTokens.Where(predicate).ToListAsync();
        }

        public void Delete(RefreshToken entity)
        {
            _context.RefreshTokens.Remove(entity);
        }
    }
}
