using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ChatMessage entity
/// </summary>
public class ChatMessageRepository : IChatMessageRepository
{
    private readonly GameDBContext _context;

    public ChatMessageRepository(GameDBContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get chat message by ID with Sender information
    /// </summary>
    public async Task<ChatMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ChatMessages
            .AsNoTracking()
            .Include(m => m.Sender) // Eager loading: Sender 정보를 즉시 로드 (LEFT JOIN)
            .SingleOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    /// <summary>
    /// Get chat messages by room ID with Cursor-based pagination
    /// </summary>
    /// <param name="roomId">Chat room ID</param>
    /// <param name="beforeId">Cursor: Get messages before this message ID (nullable)</param>
    /// <param name="take">Number of messages to retrieve (10-100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of chat messages ordered by CreatedAt DESC</returns>
    public async Task<List<ChatMessage>> GetByRoomIdAsync(
        Guid roomId,
        Guid? beforeId,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ChatMessages
            .AsNoTracking()
            .Include(m => m.Sender) // Eager loading: N+1 쿼리 방지
            .Where(m => m.RoomId == roomId);

        // Cursor 페이징: beforeId가 제공되면 해당 메시지 이전의 메시지만 조회
        if (beforeId.HasValue)
        {
            var beforeMessage = await _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.Id == beforeId.Value)
                .Select(m => m.CreatedAt)
                .SingleOrDefaultAsync(cancellationToken);

            if (beforeMessage != default)
            {
                query = query.Where(m => m.CreatedAt < beforeMessage);
            }
        }

        return await query
            .OrderByDescending(m => m.CreatedAt) // 최신순 정렬
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Add a new chat message
    /// </summary>
    public async Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        await _context.ChatMessages.AddAsync(message, cancellationToken);
    }
}
