using IdleRPG.Application.DTOs.Auth;
using IdleRPG.Application.Auth.Services;
using IdleRPG.Application.DTOs.Player;
using IdleRPG.Application.Players.Services;
using IdleRPG.Application.Tokens.Services;
using IdleRPG.Domain.Entities;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace IdleRPG.Infrastructure.Service
{
    public class AuthService : IAuthService
    {
        private readonly GameDBContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPlayerService _playerService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(GameDBContext context, IJwtTokenService jwtTokenService, IPlayerService playerService, ILogger<AuthService> logger)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _playerService = playerService;
            _logger = logger;
        }
 
        /// <summary>
        /// 회원가입 - Unity의 새 플레이어 생성과 유사
        /// </summary>
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // 1. 중복 체크 (유효성 검증)
            if (await _context.Players.AnyAsync(p => p.UserName == dto.Username))
            {
                throw new InvalidOperationException("Username already exists");
            }
            
            if (await _context.Players.AnyAsync(p => p.Email == dto.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }

            // 2. 비밀번호 해싱 (절대 평문 저장 금지!)
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            // 3. 플레이어 엔티티 생성
            
            var player = new Player
            {
                UserName = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };
            
            // 4. 기본 스탯 생성 (Unity의 AddComponent처럼)
            player.Stats = new PlayerStats
            {
                PlayerId = player.Id,
                Level = 1,
                Experience = 0,
                Gold = 1000,
                Gems = 10
            };

            player.Characters = new List<Character>()
            {
                new Character()
                {
                    Id = Guid.NewGuid(),
                    PlayerId = player.Id,
                    Player = player,
                    Name = player.UserName,
                    CharacterClass = "Warrior",
                    Level = 1,
                    Experience = 0,
                    Attack = 10,
                     Defense = 0,
                     Health = 100,
                     Mana = 10,
                     CreateAt = DateTime.UtcNow,
                     IsMain = true,
                     Inventory = new List<PlayerInventory>(),
                     OfflineRewards = new List<OfflineReward>(),
                }
            };
            
            // 5. 데이터베이스 저장
            await _playerService.CreatePlayer(player);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"New player registered: {player.UserName} (ID: {player.Id})");
            
            // 6. 토큰 생성 및 반환
            return await GenerateAuthResponse(player);
        }

        /// <summary>
        /// 로그인 - Unity의 플레이어 인증과 유사
        /// </summary>
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            // 1. 사용자 조회
            var player = await _context.Players
                .Include(p => p.Stats)
                .FirstOrDefaultAsync(p => p.UserName == dto.Username);
            
            if (player == null)
            {
                _logger.LogWarning($"Login failed: User not found ({dto.Username})");
                throw new UnauthorizedAccessException("Invalid username or password");
            }
            
            // 2. 비밀번호 검증
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, player.PasswordHash))
            {
                _logger.LogWarning($"Login failed: Wrong password for user {dto.Username}");
                throw new UnauthorizedAccessException("Invalid username or password");
            }
            
            // 3. 마지막 로그인 시간 업데이트
            player.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Player logged in: {player.UserName} (ID: {player.Id})");
            
            // 4. 토큰 생성 및 반환
            return await GenerateAuthResponse(player); 
        }

        /// <summary>
        /// Refresh Token으로 새 Access Token 발급
        /// Unity의 세션 갱신과 유사
        /// </summary>
        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            // 1. Refresh Token 조회
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.Player)
                .ThenInclude(p => p.Stats)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            
            if (storedToken == null || !storedToken.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }
            
            // 3. 새 토큰 생성
            return await GenerateAuthResponse(storedToken.Player);
        }

        /// <summary>
        /// Refresh Token 무효화 (로그아웃)
        /// </summary>
        public async Task RevokeTokenAsync(Guid userId, string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => 
                                         rt.PlayerId == userId && 
                                         rt.Token == refreshToken);
            if (storedToken != null)
            {
                storedToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Refresh token revoked for user {userId}");
            }
        }

        private async Task<AuthResponseDto> GenerateAuthResponse(Player player)
        {
            // Access Token 생성 (짧은 수명)
            var accessToken = _jwtTokenService.GenerateAccessToken(player);
            return new AuthResponseDto
            {
                AccessToken = accessToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                RefreshToken = _jwtTokenService.GenerateRefreshToken(),
                Player = new PlayerDto
                {
                    Id = player.Id,
                    UserName = player.UserName,
                }
            };
        }
    }
}