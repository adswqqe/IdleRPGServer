using IdleRPG.Application.DTOs.Chat;
using IdleRPG.Application.Interfaces;
using IdleRPG.Application.Services;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Services;

/// <summary>
/// 채팅 시스템 비즈니스 로직 구현체
/// </summary>
public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<ChatService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// 채팅 메시지를 전송합니다.
    /// </summary>
    /// <param name="roomId">채팅방 ID</param>
    /// <param name="playerId">Player ID (JWT에서 추출)</param>
    /// <param name="content">메시지 내용</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>전송된 메시지 DTO</returns>
    public async Task<ChatMessageDto> SendMessageAsync(
        Guid roomId,
        Guid playerId,
        string content,
        CancellationToken cancellationToken = default)
    {
        // 1. 입력 검증: 메시지 길이
        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogWarning("메시지 전송 실패: 빈 메시지. PlayerId={PlayerId}, RoomId={RoomId}", playerId, roomId);
            throw new ArgumentException("메시지 내용은 비워둘 수 없습니다.", nameof(content));
        }

        if (content.Length > 500)
        {
            _logger.LogWarning("메시지 전송 실패: 길이 초과. PlayerId={PlayerId}, Length={Length}", playerId, content.Length);
            throw new ArgumentException($"메시지는 최대 500자까지 입력 가능합니다. (현재: {content.Length}자)", nameof(content));
        }

        // 2. Player.Id → Character.Id 변환
        var characters = await _unitOfWork.Characters.GetByPlayerIdAsync(playerId);
        var character = characters.FirstOrDefault();

        if (character == null)
        {
            _logger.LogWarning("메시지 전송 실패: 캐릭터 미존재. PlayerId={PlayerId}", playerId);
            throw new InvalidOperationException("캐릭터가 존재하지 않습니다. 먼저 캐릭터를 생성해주세요.");
        }

        var characterId = character.Id;

        // 3. 쿨다운 체크 (1초) - Character.Id 기반
        var cacheKey = $"chat:cooldown:{characterId}";
        if (_cache.TryGetValue(cacheKey, out _))
        {
            _logger.LogWarning("메시지 전송 실패: 쿨다운 위반. CharacterId={CharacterId}", characterId);
            throw new InvalidOperationException("메시지를 너무 빠르게 전송했습니다. 1초 후에 다시 시도하세요.");
        }

        // 4. 권한 체크: 채팅방 접근 가능 여부
        var canAccess = await CanAccessRoomAsync(characterId, roomId, cancellationToken);
        if (!canAccess)
        {
            _logger.LogWarning("메시지 전송 실패: 권한 없음. CharacterId={CharacterId}, RoomId={RoomId}", characterId, roomId);
            throw new UnauthorizedAccessException("이 채팅방에 메시지를 보낼 권한이 없습니다.");
        }

        // 5. ChatMessage Entity 생성
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            SenderId = characterId, // 실제 Character.Id 사용
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        // 6. Repository에 추가 및 저장
        await _unitOfWork.ChatMessages.AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("메시지 전송 성공. MessageId={MessageId}, CharacterId={CharacterId}, RoomId={RoomId}",
            message.Id, characterId, roomId);

        // 7. 쿨다운 캐시 설정 (5초 TTL)
        _cache.Set(cacheKey, true, TimeSpan.FromSeconds(5));

        // 8. Entity → DTO 변환 (Sender 정보 포함)
        var messageWithSender = await _unitOfWork.ChatMessages.GetByIdAsync(message.Id, cancellationToken);
        if (messageWithSender == null)
        {
            throw new InvalidOperationException("메시지를 저장했으나 다시 조회할 수 없습니다.");
        }

        // TODO(human): Entity → DTO 변환 로직
        // messageWithSender.Sender에서 필요한 정보를 추출하여 ChatMessageDto를 생성하세요.
        return MapToDto(messageWithSender);
    }

    /// <summary>
    /// 채팅방의 메시지 히스토리를 조회합니다 (Cursor 페이징).
    /// </summary>
    public async Task<List<ChatMessageDto>> GetMessagesAsync(
        Guid roomId,
        Guid playerId,
        Guid? beforeId = null,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        // 1. Player.Id → Character.Id 변환
        var characters = await _unitOfWork.Characters.GetByPlayerIdAsync(playerId);
        var character = characters.FirstOrDefault();

        if (character == null)
        {
            _logger.LogWarning("메시지 조회 실패: 캐릭터 미존재. PlayerId={PlayerId}", playerId);
            throw new InvalidOperationException("캐릭터가 존재하지 않습니다. 먼저 캐릭터를 생성해주세요.");
        }

        var characterId = character.Id;

        // 2. 권한 체크: 채팅방 접근 가능 여부
        var canAccess = await CanAccessRoomAsync(characterId, roomId, cancellationToken);
        if (!canAccess)
        {
            _logger.LogWarning("메시지 조회 실패: 권한 없음. CharacterId={CharacterId}, RoomId={RoomId}", characterId, roomId);
            throw new UnauthorizedAccessException("이 채팅방의 메시지를 조회할 권한이 없습니다.");
        }

        // 3. Repository에서 메시지 조회 (Cursor 페이징)
        var messages = await _unitOfWork.ChatMessages.GetByRoomIdAsync(roomId, beforeId, take, cancellationToken);

        // 4. Entity → DTO 변환
        var dtos = messages.Select(MapToDto).ToList();

        _logger.LogInformation("메시지 조회 성공. RoomId={RoomId}, Count={Count}", roomId, dtos.Count);

        return dtos;
    }

    /// <summary>
    /// 플레이어가 특정 채팅방에 접근할 수 있는지 확인합니다.
    /// </summary>
    public async Task<bool> CanAccessRoomAsync(
        Guid playerId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        // 1. Player.Id → Character.Id 변환
        var characters = await _unitOfWork.Characters.GetByPlayerIdAsync(playerId);
        var character = characters.FirstOrDefault();

        if (character == null)
        {
            _logger.LogWarning("권한 체크 실패: 캐릭터 미존재. PlayerId={PlayerId}", playerId);
            return false;
        }

        var characterId = character.Id;

        // 2. 채팅방 조회
        var room = await _unitOfWork.ChatRooms.GetByIdAsync(roomId, cancellationToken);
        if (room == null)
        {
            _logger.LogWarning("권한 체크 실패: 채팅방 미존재. RoomId={RoomId}", roomId);
            return false;
        }

        // 3. 채팅방 타입별 권한 체크
        switch (room.Type)
        {
            case RoomType.Global:
                // Global 채팅방: 캐릭터가 존재하면 모두 접근 가능
                return true;

            case RoomType.Guild:
                // Guild 채팅방: 같은 길드 소속 캐릭터만 접근 가능
                // TODO: Guild 시스템 구현 후 활성화
                // return character.GuildId == room.GuildId;
                _logger.LogWarning("Guild 채팅방 접근 시도: Guild 시스템 미구현. RoomId={RoomId}", roomId);
                return false; // Guild 시스템 미구현

            case RoomType.Whisper:
                // Whisper 채팅방: 참여자로 등록된 캐릭터만 접근 가능
                var participants = await _unitOfWork.ChatRooms.GetByIdAsync(roomId, cancellationToken);
                if (participants == null)
                {
                    return false;
                }

                // ChatRoomParticipants에서 characterId가 있는지 확인
                return participants.Participants.Any(p => p.CharacterId == characterId);

            default:
                _logger.LogError("알 수 없는 채팅방 타입. RoomType={RoomType}, RoomId={RoomId}", room.Type, roomId);
                return false;
        }
    }

    /// <summary>
    /// 캐릭터가 접근 가능한 모든 채팅방 목록을 조회합니다.
    /// </summary>
    public async Task<List<ChatRoomDto>> GetAccessibleRoomsAsync(
        Guid playerId,
        CancellationToken cancellationToken = default)
    {
        // 1. Player.Id → Character.Id 변환 및 캐릭터 조회 (존재 여부 확인)
        var characters = await _unitOfWork.Characters.GetByPlayerIdAsync(playerId);
        var character = characters.FirstOrDefault();

        if (character == null)
        {
            _logger.LogWarning("채팅방 목록 조회 실패: 캐릭터 미존재. PlayerId={PlayerId}", playerId);
            throw new KeyNotFoundException($"캐릭터를 찾을 수 없습니다. (PlayerId: {playerId})");
        }

        var characterId = character.Id;

        // 2. Repository에서 접근 가능한 채팅방 조회
        // TODO: Guild 시스템 구현 후 character.GuildId 전달
        var rooms = await _unitOfWork.ChatRooms.GetAccessibleRoomsAsync(characterId, null, cancellationToken);

        // 3. Entity → DTO 변환
        var dtos = rooms.Select(room => new ChatRoomDto
        {
            Id = room.Id,
            Type = room.Type.ToString(), // Enum → string
            Name = room.Name
            // LastMessage는 미래 확장 (null)
        }).ToList();

        _logger.LogInformation("채팅방 목록 조회 성공. CharacterId={CharacterId}, Count={Count}", characterId, dtos.Count);

        return dtos;
    }

    /// <summary>
    /// 두 캐릭터 간의 1:1 귓속말 채팅방을 생성하거나 기존 방을 조회합니다.
    /// </summary>
    public async Task<Guid> CreateWhisperRoomAsync(
        Guid characterAId,
        Guid characterBId,
        CancellationToken cancellationToken = default)
    {
        // 1. 입력 검증: 자기 자신과 귓속말 방지
        if (characterAId == characterBId)
        {
            _logger.LogWarning("Whisper 방 생성 실패: 자기 자신과 귓속말 시도. CharacterId={CharacterId}", characterAId);
            throw new InvalidOperationException("자기 자신과는 귓속말을 할 수 없습니다.");
        }

        // 2. 캐릭터 존재 여부 확인
        var characterA = await _unitOfWork.Characters.GetByIdAsync(characterAId);
        var characterB = await _unitOfWork.Characters.GetByIdAsync(characterBId);

        if (characterA == null)
        {
            _logger.LogWarning("Whisper 방 생성 실패: 캐릭터 A 미존재. CharacterId={CharacterId}", characterAId);
            throw new KeyNotFoundException($"캐릭터를 찾을 수 없습니다. (ID: {characterAId})");
        }

        if (characterB == null)
        {
            _logger.LogWarning("Whisper 방 생성 실패: 캐릭터 B 미존재. CharacterId={CharacterId}", characterBId);
            throw new KeyNotFoundException($"캐릭터를 찾을 수 없습니다. (ID: {characterBId})");
        }

        // 3. 기존 Whisper 방 확인 (A-B 또는 B-A)
        // 설계 선택: 2번 단순 쿼리 (가독성 우선)
        // 대안: 1번 복잡한 JOIN 쿼리 (성능 우선)
        var existingRooms = await _unitOfWork.ChatRooms.GetAccessibleRoomsAsync(characterAId, null, cancellationToken);
        var existingWhisperRoom = existingRooms
            .Where(r => r.Type == RoomType.Whisper)
            .FirstOrDefault(r => r.Participants.Any(p => p.CharacterId == characterBId));

        if (existingWhisperRoom != null)
        {
            _logger.LogInformation("기존 Whisper 방 재사용. RoomId={RoomId}, CharacterA={CharacterA}, CharacterB={CharacterB}",
                existingWhisperRoom.Id, characterAId, characterBId);
            return existingWhisperRoom.Id;
        }

        // 4. 새로운 Whisper 방 생성
        var newRoom = new ChatRoom
        {
            Id = Guid.NewGuid(),
            Type = RoomType.Whisper,
            Name = $"Whisper: {characterAId:N} ↔ {characterBId:N}", // Character에 Name 없음, ID 사용
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ChatRooms.AddAsync(newRoom, cancellationToken);

        // 5. 참여자 2명 추가
        var participantA = new ChatRoomParticipant
        {
            Id = Guid.NewGuid(),
            RoomId = newRoom.Id,
            CharacterId = characterAId,
            JoinedAt = DateTime.UtcNow
        };

        var participantB = new ChatRoomParticipant
        {
            Id = Guid.NewGuid(),
            RoomId = newRoom.Id,
            CharacterId = characterBId,
            JoinedAt = DateTime.UtcNow
        };

        // 참여자 추가 (UnitOfWork에 ChatRoomParticipants Repository가 없으므로 직접 접근)
        // TODO: IChatRoomParticipantRepository 추가 또는 ChatRooms.AddParticipantAsync 메서드 추가
        newRoom.Participants.Add(participantA);
        newRoom.Participants.Add(participantB);

        // 6. 트랜잭션 커밋 (SaveChangesAsync 1번 호출로 원자성 보장)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Whisper 방 생성 성공. RoomId={RoomId}, CharacterA={CharacterA}, CharacterB={CharacterB}",
            newRoom.Id, characterAId, characterBId);

        return newRoom.Id;
    }

    // ========== Private Helper Methods ==========

    /// <summary>
    /// ChatMessage Entity를 ChatMessageDto로 변환합니다.
    /// </summary>
    private ChatMessageDto MapToDto(ChatMessage message)
    {
        return new ChatMessageDto
        {
            Id = message.Id,
            RoomId = message.RoomId,
            Sender = new CharacterSummaryDto
            {
                Id = message.Sender.Id,
                Name = $"Character-{message.Sender.Id:N}", // TODO: Character에 Name 필드 추가 필요
                Level = message.Sender.Level
            },
            Content = message.Content,
            CreatedAt = message.CreatedAt
            // Reactions, IsEdited, ParentMessageId는 미래 확장 필드 (null)
        };
    }
}
