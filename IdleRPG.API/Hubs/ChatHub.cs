using IdleRPG.Application.DTOs.Chat;
using IdleRPG.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace IdleRPG.API.Hubs;

/// <summary>
/// 실시간 채팅을 위한 SignalR Hub
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// 채팅방에 참여합니다.
    /// </summary>
    /// <param name="roomId">채팅방 ID (문자열 형식)</param>
    public async Task JoinRoom(string roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId) || !Guid.TryParse(roomId, out var roomGuid))
        {
            _logger.LogWarning("JoinRoom: Invalid roomId format. RoomId={RoomId}", roomId);
            await Clients.Caller.SendAsync("Error", new ErrorDto
            {
                Code = "INVALID_ROOM_ID",
                Message = "Invalid room ID format"
            });
            return;
        }

        var characterId = GetCharacterIdFromContext();
        if (characterId == Guid.Empty)
        {
            _logger.LogWarning("JoinRoom: CharacterId extraction failed");
            await Clients.Caller.SendAsync("Error", new ErrorDto
            {
                Code = "UNAUTHORIZED",
                Message = "Authentication failed"
            });
            return;
        }

        try
        {
            // 권한 체크
            var canAccess = await _chatService.CanAccessRoomAsync(characterId, roomGuid);
            if (!canAccess)
            {
                _logger.LogWarning(
                    "JoinRoom: Access denied. CharacterId={CharacterId}, RoomId={RoomId}",
                    characterId,
                    roomGuid);

                await Clients.Caller.SendAsync("Error", new ErrorDto
                {
                    Code = "FORBIDDEN",
                    Message = "You don't have access to this room"
                });
                return;
            }

            // SignalR 그룹에 추가
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            _logger.LogInformation(
                "JoinRoom: Success. CharacterId={CharacterId}, RoomId={RoomId}, ConnectionId={ConnectionId}",
                characterId,
                roomGuid,
                Context.ConnectionId);

            // 다른 참여자들에게 알림 (선택적)
            await Clients.OthersInGroup(roomId).SendAsync("UserJoined", new
            {
                CharacterId = characterId,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "JoinRoom: Unexpected error. CharacterId={CharacterId}, RoomId={RoomId}",
                characterId,
                roomGuid);

            await Clients.Caller.SendAsync("Error", new ErrorDto
            {
                Code = "SERVER_ERROR",
                Message = "Failed to join room"
            });
        }
    }

    /// <summary>
    /// 채팅방에서 나갑니다.
    /// </summary>
    /// <param name="roomId">채팅방 ID (문자열 형식)</param>
    public async Task LeaveRoom(string roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId) || !Guid.TryParse(roomId, out var roomGuid))
        {
            _logger.LogWarning("LeaveRoom: Invalid roomId format. RoomId={RoomId}", roomId);
            return;
        }

        var characterId = GetCharacterIdFromContext();
        if (characterId == Guid.Empty)
        {
            _logger.LogWarning("LeaveRoom: CharacterId extraction failed");
            return;
        }

        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

            _logger.LogInformation(
                "LeaveRoom: Success. CharacterId={CharacterId}, RoomId={RoomId}, ConnectionId={ConnectionId}",
                characterId,
                roomGuid,
                Context.ConnectionId);

            // 다른 참여자들에게 알림 (선택적)
            await Clients.OthersInGroup(roomId).SendAsync("UserLeft", new
            {
                CharacterId = characterId,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "LeaveRoom: Unexpected error. CharacterId={CharacterId}, RoomId={RoomId}",
                characterId,
                roomGuid);
        }
    }

    /// <summary>
    /// 채팅 메시지를 전송합니다.
    /// </summary>
    /// <param name="roomId">채팅방 ID (문자열 형식)</param>
    /// <param name="content">메시지 내용 (1-500자)</param>
    public async Task SendMessage(string roomId, string content)
    {
        if (string.IsNullOrWhiteSpace(roomId) || !Guid.TryParse(roomId, out var roomGuid))
        {
            _logger.LogWarning("SendMessage: Invalid roomId format. RoomId={RoomId}", roomId);
            await Clients.Caller.SendAsync("Error", new ErrorDto
            {
                Code = "INVALID_ROOM_ID",
                Message = "Invalid room ID format"
            });
            return;
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogWarning("SendMessage: Empty message content");
            await Clients.Caller.SendAsync("Error", new ErrorDto
            {
                Code = "INVALID_MESSAGE",
                Message = "Message content cannot be empty"
            });
            return;
        }

        var characterId = GetCharacterIdFromContext();
        if (characterId == Guid.Empty)
        {
            _logger.LogWarning("SendMessage: CharacterId extraction failed");
            await Clients.Caller.SendAsync("Error", new ErrorDto
            {
                Code = "UNAUTHORIZED",
                Message = "Authentication failed"
            });
            return;
        }

        try
        {
            // 메시지 저장 및 전송 (쿨다운 체크 포함)
            var messageDto = await _chatService.SendMessageAsync(
                roomGuid,
                characterId,
                content,
                Context.ConnectionAborted);

            // 같은 방의 모든 클라이언트에게 브로드캐스트
            await Clients.Group(roomId).SendAsync("ReceiveMessage", messageDto);

            _logger.LogInformation(
                "SendMessage: Success. CharacterId={CharacterId}, RoomId={RoomId}, MessageId={MessageId}",
                characterId,
                roomGuid,
                messageDto.Id);
        }
        catch (InvalidOperationException ex)
        {
            // 쿨다운 위반
            _logger.LogWarning(
                ex,
                "SendMessage: Cooldown violation. CharacterId={CharacterId}, RoomId={RoomId}",
                characterId,
                roomGuid);

            await Clients.Caller.SendAsync("Error", new ErrorDto
            {
                Code = "COOLDOWN_ACTIVE",
                Message = "Please wait before sending another message"
            });
        }
        catch (ArgumentException ex)
        {
            // 메시지 길이 초과
            _logger.LogWarning(
                ex,
                "SendMessage: Invalid message. CharacterId={CharacterId}, RoomId={RoomId}",
                characterId,
                roomGuid);

            await Clients.Caller.SendAsync("Error", new ErrorDto
            {
                Code = "INVALID_MESSAGE",
                Message = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            // 권한 없음
            _logger.LogWarning(
                ex,
                "SendMessage: Access denied. CharacterId={CharacterId}, RoomId={RoomId}",
                characterId,
                roomGuid);

            await Clients.Caller.SendAsync("Error", new ErrorDto
            {
                Code = "FORBIDDEN",
                Message = "You don't have access to this room"
            });
        }
        catch (Exception ex)
        {
            // 예상치 못한 에러
            _logger.LogError(
                ex,
                "SendMessage: Unexpected error. CharacterId={CharacterId}, RoomId={RoomId}",
                characterId,
                roomGuid);

            await Clients.Caller.SendAsync("Error", new ErrorDto
            {
                Code = "SERVER_ERROR",
                Message = "Failed to send message"
            });
        }
    }

    /// <summary>
    /// 사용자가 타이핑 중임을 알립니다 (미래 확장 기능).
    /// </summary>
    /// <param name="roomId">채팅방 ID (문자열 형식)</param>
    public async Task Typing(string roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId) || !Guid.TryParse(roomId, out var roomGuid))
        {
            return;
        }

        var characterId = GetCharacterIdFromContext();
        if (characterId == Guid.Empty)
        {
            return;
        }

        // 자신을 제외한 같은 방의 사용자들에게만 알림
        await Clients.OthersInGroup(roomId).SendAsync("UserTyping", new
        {
            CharacterId = characterId,
            RoomId = roomGuid,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogDebug(
            "Typing: CharacterId={CharacterId}, RoomId={RoomId}",
            characterId,
            roomGuid);
    }

    /// <summary>
    /// JWT에서 CharacterId를 추출합니다.
    /// </summary>
    /// <returns>CharacterId (추출 실패 시 Guid.Empty)</returns>
    private Guid GetCharacterIdFromContext()
    {
        var characterIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(characterIdClaim) || !Guid.TryParse(characterIdClaim, out var characterId))
        {
            return Guid.Empty;
        }

        return characterId;
    }

    /// <summary>
    /// 클라이언트 연결 시 호출됩니다.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var characterId = GetCharacterIdFromContext();
        _logger.LogInformation(
            "OnConnectedAsync: CharacterId={CharacterId}, ConnectionId={ConnectionId}",
            characterId,
            Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// 클라이언트 연결 해제 시 호출됩니다.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var characterId = GetCharacterIdFromContext();
        _logger.LogInformation(
            exception,
            "OnDisconnectedAsync: CharacterId={CharacterId}, ConnectionId={ConnectionId}",
            characterId,
            Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }
}
