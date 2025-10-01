using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace IdleRPG.Infrastructure.Repositories
{
    public class CharacterRepository : ICharacterRepository
    {
        private readonly GameDBContext _context;

        public CharacterRepository(GameDBContext context)
        {
            _context = context;
        }
        
        public Task<Character?> GetByIdAsync(Guid id)
        {
            return _context.Characters.SingleOrDefaultAsync(c => c.Id == id);
        }

        public Task<List<Character>> GetByPlayerIdAsync(Guid playerId)
        {
            return _context.Characters.Where(c => c.PlayerId == playerId).ToListAsync();
        }

        public async Task AddAsync(Character character)
        {
            await _context.Characters.AddAsync(character);
        }

        public void Update(Character character)
        {
            _context.Characters.Update(character);
        }

        public void Delete(Character character)
        {
            _context.Characters.Remove(character);
        }

        public Task<bool> ExistsAsync(Guid id)
        {
            return _context.Characters.AnyAsync(c => c.Id == id);
        }
    }
}