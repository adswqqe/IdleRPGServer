using IdleRPG.Application.DTOs.Player;
using IdleRPG.Domain.Entities;
namespace IdleRPG.Application.Players.Services
{
    public interface IPlayerService
    {
        Task<PlayerDto>  CreatePlayer(Player player);
        Task<PlayerDto> GetPlayer(Guid id);
        Task UpdatePlayer(UpdatePlayerDto updatePlayerDto);
    }
}