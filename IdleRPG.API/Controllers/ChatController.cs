using IdleRPG.Application.DTOs.Chat;
using IdleRPG.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdleRPG.API.Controllers;

/// <summary>
/// 채팅 시스템 REST API 엔드포인트
/// </summary>
[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// 채팅방의 메시지 히스토리를 조회합니다 (Cursor 페이징).
    /// </summary>
    /// <param name="roomId">채팅방 ID</param>
    /// <param name="beforeId">이전 메시지 ID (null이면 최신 메시지부터)</param>
    /// <param name="take">조회할 메시지 개수 (기본 50, 최소 10, 최대 100)</param>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>메시지 목록 (최신순 정렬)</returns>
    /// <response code="200">메시지 목록 반환 성공</response>
    /// <response code="400">잘못된 요청 (roomId 형식 오류, take 범위 초과)</response>
    /// <response code="401">인증 실패 (JWT 토큰 없음/만료)</response>
    /// <response code="403">권한 없음 (채팅방 접근 불가)</response>
    /// <response code="404">채팅방 미존재</response>
    [HttpGet("rooms/{roomId}/messages")]
    [ProducesResponseType(typeof(List<ChatMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ChatMessageDto>>> GetMessages(
        [FromRoute] Guid roomId,
        [FromQuery] Guid? beforeId = null,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        // Input Validation
        if (roomId == Guid.Empty)
        {
            _logger.LogWarning("GetMessages called with empty roomId");
            return BadRequest(new { error = "Invalid roomId" });
        }

        if (take < 10 || take > 100)
        {
            _logger.LogWarning("GetMessages called with invalid take value: {Take}", take);
            return BadRequest(new { error = "take must be between 10 and 100" });
        }

        // JWT에서 CharacterId 추출
        var characterIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(characterIdClaim) || !Guid.TryParse(characterIdClaim, out var characterId))
        {
            _logger.LogWarning("GetMessages: Invalid or missing CharacterId in JWT token");
            return Unauthorized(new { error = "Invalid authentication token" });
        }

        try
        {
            var messages = await _chatService.GetMessagesAsync(
                roomId,
                characterId,
                beforeId,
                take,
                cancellationToken);

            _logger.LogInformation(
                "GetMessages: CharacterId={CharacterId}, RoomId={RoomId}, Count={Count}",
                characterId,
                roomId,
                messages.Count);

            return Ok(messages);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "GetMessages: Room not found. RoomId={RoomId}", roomId);
            return NotFound(new { error = $"Chat room not found: {roomId}" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(
                ex,
                "GetMessages: Access denied. CharacterId={CharacterId}, RoomId={RoomId}",
                characterId,
                roomId);
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Access denied to this chat room" });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "GetMessages: Unexpected error. CharacterId={CharacterId}, RoomId={RoomId}",
                characterId,
                roomId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// 캐릭터가 접근 가능한 모든 채팅방 목록을 조회합니다.
    /// </summary>
    /// <param name="cancellationToken">작업 취소 토큰</param>
    /// <returns>접근 가능한 채팅방 목록 (Global + Guild + Whisper)</returns>
    /// <response code="200">채팅방 목록 반환 성공</response>
    /// <response code="401">인증 실패 (JWT 토큰 없음/만료)</response>
    /// <response code="404">캐릭터 미존재</response>
    [HttpGet("rooms")]
    [ProducesResponseType(typeof(List<ChatRoomDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ChatRoomDto>>> GetRooms(CancellationToken cancellationToken = default)
    {
        // JWT에서 CharacterId 추출
        var characterIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(characterIdClaim) || !Guid.TryParse(characterIdClaim, out var characterId))
        {
            _logger.LogWarning("GetRooms: Invalid or missing CharacterId in JWT token");
            return Unauthorized(new { error = "Invalid authentication token" });
        }

        try
        {
            var rooms = await _chatService.GetAccessibleRoomsAsync(characterId, cancellationToken);

            _logger.LogInformation(
                "GetRooms: CharacterId={CharacterId}, RoomCount={Count}",
                characterId,
                rooms.Count);

            return Ok(rooms);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "GetRooms: Character not found. CharacterId={CharacterId}", characterId);
            return NotFound(new { error = $"Character not found: {characterId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRooms: Unexpected error. CharacterId={CharacterId}", characterId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }
}
