using IdleRPG.Application.DTOs.Auth;
using IdleRPG.Application.Auth.Services;
using IdleRPG.Application.Tokens.Services;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (!await _unitOfWork.Players.IsUsernameAvailableAsync(dto.Username))
                throw new InvalidOperationException("Username already exists");

            var player = new Player
            {
                UserName = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            await _unitOfWork.Players.AddAsync(player);

            var accessToken = _jwtTokenService.GenerateAccessToken(player);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
            {
                Token = refreshToken,
                PlayerId = player.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });

            // 한 번의 트랜잭션으로 Player와 RefreshToken 모두 저장
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // 1시간 = 3600초
                PlayerId = player.Id,
                UserName = player.UserName
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var player = await _unitOfWork.Players.GetByUsernameAsync(dto.Username);

            if (player == null || !BCrypt.Net.BCrypt.Verify(dto.Password, player.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            player.LastLoginAt = DateTime.UtcNow;

            var accessToken = _jwtTokenService.GenerateAccessToken(player);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
            {
                Token = refreshToken,
                PlayerId = player.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });

            // 한 번의 트랜잭션으로 LastLoginAt 업데이트와 RefreshToken 생성 모두 저장
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // 1시간 = 3600초
                PlayerId = player.Id,
                UserName = player.UserName
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var token = await _unitOfWork.RefreshTokens.GetByTokenWithPlayerAsync(refreshToken);

            if (token == null || token.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token");

            var player = token.Player;
            var newAccessToken = _jwtTokenService.GenerateAccessToken(player);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            _unitOfWork.RefreshTokens.Delete(token);
            await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
            {
                Token = newRefreshToken,
                PlayerId = player.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });

            // 한 번의 트랜잭션으로 이전 토큰 제거 및 새 토큰 생성
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 3600, // 1시간 = 3600초
                PlayerId = player.Id,
                UserName = player.UserName
            };
        }

        public async Task RevokeTokenAsync(Guid userId, string refreshToken)
        {
            var token = await _unitOfWork.RefreshTokens.GetByPlayerIdAndTokenAsync(userId, refreshToken);

            if (token != null)
            {
                _unitOfWork.RefreshTokens.Delete(token);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
