using IdleRPG.Application.DTOs.Auth;
using IdleRPG.Application.Auth.Services;
using IdleRPG.Application.Tokens.Services;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Service
{
    public class AuthService : IAuthService
    {
        private readonly GameDBContext _context;
        private readonly IPlayerRepository _playerRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(GameDBContext context, IPlayerRepository playerRepository, IJwtTokenService jwtTokenService, ILogger<AuthService> logger)
        {
            _context = context;
            _playerRepository = playerRepository;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (!await _playerRepository.IsUsernameAvailableAsync(dto.Username))
                throw new InvalidOperationException("Username already exists");

            var player = new Player
            {
                UserName = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            await _playerRepository.AddAsync(player);
            await _playerRepository.SaveChangesAsync();

            var accessToken = _jwtTokenService.GenerateAccessToken(player);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                PlayerId = player.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });
            await _playerRepository.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                PlayerId = player.Id,
                UserName = player.UserName
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var player = await _playerRepository.GetByUsernameAsync(dto.Username);

            if (player == null || !BCrypt.Net.BCrypt.Verify(dto.Password, player.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            player.LastLoginAt = DateTime.UtcNow;
            await _playerRepository.SaveChangesAsync();

            var accessToken = _jwtTokenService.GenerateAccessToken(player);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                PlayerId = player.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });
            await _playerRepository.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                PlayerId = player.Id,
                UserName = player.UserName
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens.Include(rt => rt.Player)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null || token.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token");

            var player = token.Player;
            var newAccessToken = _jwtTokenService.GenerateAccessToken(player);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            _context.RefreshTokens.Remove(token);
            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                PlayerId = player.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });
            await _playerRepository.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                PlayerId = player.Id,
                UserName = player.UserName
            };
        }

        public async Task RevokeTokenAsync(Guid userId, string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.PlayerId == userId && rt.Token == refreshToken);

            if (token != null)
            {
                _context.RefreshTokens.Remove(token);
                await _playerRepository.SaveChangesAsync();
            }
        }
    }
}
