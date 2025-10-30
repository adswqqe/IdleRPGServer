using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ChatRoom entity
/// </summary>
public class ChatRoomRepository : IChatRoomRepository
{
    private readonly GameDBContext _context;

    public ChatRoomRepository(GameDBContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get chat room by ID
    /// </summary>
    public async Task<ChatRoom?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ChatRooms
            .AsNoTracking()
            // TODO: Guild Entity 구현 후 주석 제거
            // .Include(r => r.Guild) // Guild 정보를 즉시 로드 (nullable이므로 LEFT JOIN으로 처리)
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <summary>
    /// Get accessible chat rooms for a character
    /// </summary>
    /// <param name="characterId">Character ID requesting access</param>
    /// <param name="guildId">Guild ID of the character (nullable)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of accessible chat rooms</returns>
    public async Task<List<ChatRoom>> GetAccessibleRoomsAsync(
        Guid characterId,
        Guid? guildId,
        CancellationToken cancellationToken = default)
    {
        var rooms = new List<ChatRoom>();

        // 1. Global 타입 방 (모든 인증 사용자 접근 가능)
        var globalRooms = await _context.ChatRooms
            .AsNoTracking()
            .Where(r => r.Type == RoomType.Global)
            .ToListAsync(cancellationToken);
        rooms.AddRange(globalRooms);

        // 2. Guild 타입 방 (같은 GuildId를 가진 캐릭터만)
        if (guildId.HasValue)
        {
            var guildRooms = await _context.ChatRooms
                .AsNoTracking()
                .Where(r => r.Type == RoomType.Guild && r.GuildId == guildId.Value)
                .ToListAsync(cancellationToken);
            rooms.AddRange(guildRooms);
        }

        // 3. Whisper 타입 방 (ChatRoomParticipants에서 characterId가 참여한 방)
        var whisperRoomIds = await _context.ChatRoomParticipants
            .AsNoTracking()
            .Where(p => p.CharacterId == characterId)
            .Select(p => p.RoomId)
            .ToListAsync(cancellationToken);

        var whisperRooms = await _context.ChatRooms
            .AsNoTracking()
            .Where(r => r.Type == RoomType.Whisper && whisperRoomIds.Contains(r.Id))
            .ToListAsync(cancellationToken);
        rooms.AddRange(whisperRooms);

        return rooms;
    }

    /// <summary>
    /// Add a new chat room
    /// </summary>
    public async Task AddAsync(ChatRoom room, CancellationToken cancellationToken = default)
    {
        await _context.ChatRooms.AddAsync(room, cancellationToken);
    }
}
