using IdleRPG.Application.DTOs.Player;
using IdleRPG.Application.Players.Services;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
namespace IdleRPG.Infrastructure.Service
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        
        public PlayerService(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }
        
        public async Task<PlayerDto> CreatePlayer(Player player)
        {
            await _playerRepository.AddAsync(player);
            
            return new PlayerDto()
            {
                Id = player.Id,
                UserName = player.UserName,
                CurrentStage = player.CurrentStage,
                TotalIdleTime = player.TotalIdleTime,
                Characters = player.Characters,
                Stats = player.Stats,
            };
        }

        public async Task<PlayerDto> GetPlayer(Guid id)
        {
            var player = await _playerRepository.GetPlayerWithCharactersAsync(id);

            if (player == null)
                throw new KeyNotFoundException($"Player with ID {id} not found");
            
            return new PlayerDto()
            {
                Id = player.Id,
                UserName = player.UserName,
                CurrentStage = player.CurrentStage,
                TotalIdleTime = player.TotalIdleTime,
                Characters = player.Characters,
                Stats = player.Stats,
            };
        }

        public async Task UpdatePlayer(UpdatePlayerDto updatePlayerDto)
        {
            var player = await _playerRepository.GetPlayerWithCharactersForUpdateAsync(updatePlayerDto.Id);

            if (player == null)
                throw new KeyNotFoundException($"Player with ID {updatePlayerDto.Id} not found");
            
            player.UserName = updatePlayerDto.UserName;
            await _playerRepository.UpdateAsync(player);
        }
    }
}