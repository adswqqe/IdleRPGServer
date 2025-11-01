using FluentAssertions;
using IdleRPG.API.Controllers;
using IdleRPG.Application.DTOs.Chat;
using IdleRPG.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace IdleRPG.Tests.API.Controllers;

/// <summary>
/// ChatController 통합 테스트
/// API 레이어: HTTP 응답, JWT 인증, 에러 처리
/// </summary>
public class ChatControllerTests
{
    private readonly Mock<IChatService> _mockChatService;
    private readonly Mock<ILogger<ChatController>> _mockLogger;
    private readonly ChatController _controller;
    private readonly Guid _testCharacterId;
    private readonly Guid _testRoomId;

    public ChatControllerTests()
    {
        _mockChatService = new Mock<IChatService>();
        _mockLogger = new Mock<ILogger<ChatController>>();
        _controller = new ChatController(_mockChatService.Object, _mockLogger.Object);

        _testCharacterId = Guid.NewGuid();
        _testRoomId = Guid.NewGuid();

        // ControllerContext 설정 (JWT Claim Mock)
        SetupControllerContext(_testCharacterId);
    }

    // ========== Helper Methods ==========

    /// <summary>
    /// Controller에 ControllerContext 설정 (JWT Claim Mock)
    /// </summary>
    private void SetupControllerContext(Guid characterId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, characterId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    /// <summary>
    /// JWT Claim이 없는 ControllerContext 설정 (인증 실패 시나리오)
    /// </summary>
    private void SetupControllerContextWithoutClaim()
    {
        var identity = new ClaimsIdentity(); // 빈 Identity (인증 없음)
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    // ========== GetMessages Tests ==========

    [Fact]
    public async Task GetMessages_ValidRequest_Returns200WithMessageList()
    {
        // Arrange: 정상 요청 시나리오
        var beforeId = Guid.NewGuid();
        var take = 50;
        var cancellationToken = CancellationToken.None;

        var expectedMessages = new List<ChatMessageDto>
        {
            new ChatMessageDto
            {
                Id = Guid.NewGuid(),
                RoomId = _testRoomId,
                Sender = new CharacterSummaryDto
                {
                    Id = _testCharacterId,
                    Name = "TestUser",
                    Level = 10
                },
                Content = "Hello!",
                CreatedAt = DateTime.UtcNow
            },
            new ChatMessageDto
            {
                Id = Guid.NewGuid(),
                RoomId = _testRoomId,
                Sender = new CharacterSummaryDto
                {
                    Id = _testCharacterId,
                    Name = "TestUser",
                    Level = 10
                },
                Content = "How are you?",
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            }
        };

        _mockChatService
            .Setup(s => s.GetMessagesAsync(_testRoomId, _testCharacterId, beforeId, take, cancellationToken))
            .ReturnsAsync(expectedMessages);

        // Act: 메시지 조회
        var result = await _controller.GetMessages(_testRoomId, beforeId, take, cancellationToken);

        // Assert: 200 OK, 메시지 리스트 반환
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var messages = okResult!.Value as List<ChatMessageDto>;
        messages.Should().HaveCount(2);
        messages![0].Content.Should().Be("Hello!");
        messages[1].Content.Should().Be("How are you?");

        // Mock 호출 검증
        _mockChatService.Verify(
            s => s.GetMessagesAsync(_testRoomId, _testCharacterId, beforeId, take, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetMessages_WithoutBeforeId_Returns200()
    {
        // Arrange: beforeId 없이 최신 메시지 조회
        var take = 30;
        var cancellationToken = CancellationToken.None;

        var expectedMessages = new List<ChatMessageDto>
        {
            new ChatMessageDto
            {
                Id = Guid.NewGuid(),
                RoomId = _testRoomId,
                Sender = new CharacterSummaryDto { Id = _testCharacterId, Name = "Test", Level = 5 },
                Content = "Latest message",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockChatService
            .Setup(s => s.GetMessagesAsync(_testRoomId, _testCharacterId, null, take, cancellationToken))
            .ReturnsAsync(expectedMessages);

        // Act: beforeId 없이 조회
        var result = await _controller.GetMessages(_testRoomId, null, take, cancellationToken);

        // Assert: 200 OK
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var messages = okResult!.Value as List<ChatMessageDto>;
        messages.Should().HaveCount(1);
        messages![0].Content.Should().Be("Latest message");
    }

    [Fact]
    public async Task GetMessages_EmptyRoomId_Returns400BadRequest()
    {
        // Arrange: 빈 roomId (Guid.Empty)
        var emptyRoomId = Guid.Empty;
        var take = 50;
        var cancellationToken = CancellationToken.None;

        // Act: 빈 roomId로 요청
        var result = await _controller.GetMessages(emptyRoomId, null, take, cancellationToken);

        // Assert: 400 Bad Request
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();

        // Service 호출되지 않음
        _mockChatService.Verify(
            s => s.GetMessagesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMessages_InvalidTakeValue_Returns400BadRequest()
    {
        // Arrange: take 범위 초과 (최소 10, 최대 100)
        var invalidTake = 5; // 10보다 작음
        var cancellationToken = CancellationToken.None;

        // Act: 잘못된 take 값으로 요청
        var result = await _controller.GetMessages(_testRoomId, null, invalidTake, cancellationToken);

        // Assert: 400 Bad Request
        result.Result.Should().BeOfType<BadRequestObjectResult>();

        // Service 호출되지 않음
        _mockChatService.Verify(
            s => s.GetMessagesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMessages_TakeTooLarge_Returns400BadRequest()
    {
        // Arrange: take 값이 100 초과
        var invalidTake = 101;
        var cancellationToken = CancellationToken.None;

        // Act: 잘못된 take 값으로 요청
        var result = await _controller.GetMessages(_testRoomId, null, invalidTake, cancellationToken);

        // Assert: 400 Bad Request
        result.Result.Should().BeOfType<BadRequestObjectResult>();

        // Service 호출되지 않음
        _mockChatService.Verify(
            s => s.GetMessagesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMessages_NoJwtToken_Returns401Unauthorized()
    {
        // Arrange: JWT Claim 없음
        SetupControllerContextWithoutClaim();

        var take = 50;
        var cancellationToken = CancellationToken.None;

        // Act: JWT 없이 요청
        var result = await _controller.GetMessages(_testRoomId, null, take, cancellationToken);

        // Assert: 401 Unauthorized
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();

        // Service 호출되지 않음
        _mockChatService.Verify(
            s => s.GetMessagesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMessages_RoomNotFound_Returns404NotFound()
    {
        // Arrange: 존재하지 않는 채팅방
        var take = 50;
        var cancellationToken = CancellationToken.None;

        _mockChatService
            .Setup(s => s.GetMessagesAsync(_testRoomId, _testCharacterId, null, take, cancellationToken))
            .ThrowsAsync(new KeyNotFoundException($"Chat room not found: {_testRoomId}"));

        // Act: 존재하지 않는 방 조회
        var result = await _controller.GetMessages(_testRoomId, null, take, cancellationToken);

        // Assert: 404 Not Found
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetMessages_AccessDenied_Returns403Forbidden()
    {
        // Arrange: 권한 없는 채팅방 접근
        var take = 50;
        var cancellationToken = CancellationToken.None;

        _mockChatService
            .Setup(s => s.GetMessagesAsync(_testRoomId, _testCharacterId, null, take, cancellationToken))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied to this chat room"));

        // Act: 권한 없는 방 접근
        var result = await _controller.GetMessages(_testRoomId, null, take, cancellationToken);

        // Assert: 403 Forbidden
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task GetMessages_ServiceException_Returns500InternalServerError()
    {
        // Arrange: Service에서 예기치 않은 예외 발생
        var take = 50;
        var cancellationToken = CancellationToken.None;

        _mockChatService
            .Setup(s => s.GetMessagesAsync(_testRoomId, _testCharacterId, null, take, cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act: Service 예외 발생
        var result = await _controller.GetMessages(_testRoomId, null, take, cancellationToken);

        // Assert: 500 Internal Server Error
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // ========== GetRooms Tests ==========

    [Fact]
    public async Task GetRooms_ValidRequest_Returns200WithRoomList()
    {
        // Arrange: 정상 요청 시나리오
        var cancellationToken = CancellationToken.None;

        var expectedRooms = new List<ChatRoomDto>
        {
            new ChatRoomDto
            {
                Id = Guid.NewGuid(),
                Type = "Global",
                Name = "전체 채팅"
            },
            new ChatRoomDto
            {
                Id = Guid.NewGuid(),
                Type = "Whisper",
                Name = "Whisper Room"
            }
        };

        _mockChatService
            .Setup(s => s.GetAccessibleRoomsAsync(_testCharacterId, cancellationToken))
            .ReturnsAsync(expectedRooms);

        // Act: 채팅방 목록 조회
        var result = await _controller.GetRooms(cancellationToken);

        // Assert: 200 OK, 채팅방 리스트 반환
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var rooms = okResult!.Value as List<ChatRoomDto>;
        rooms.Should().HaveCount(2);
        rooms![0].Type.Should().Be("Global");
        rooms[1].Type.Should().Be("Whisper");

        // Mock 호출 검증
        _mockChatService.Verify(
            s => s.GetAccessibleRoomsAsync(_testCharacterId, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetRooms_NoJwtToken_Returns401Unauthorized()
    {
        // Arrange: JWT Claim 없음
        SetupControllerContextWithoutClaim();

        var cancellationToken = CancellationToken.None;

        // Act: JWT 없이 요청
        var result = await _controller.GetRooms(cancellationToken);

        // Assert: 401 Unauthorized
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();

        // Service 호출되지 않음
        _mockChatService.Verify(
            s => s.GetAccessibleRoomsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetRooms_CharacterNotFound_Returns404NotFound()
    {
        // Arrange: 존재하지 않는 캐릭터
        var cancellationToken = CancellationToken.None;

        _mockChatService
            .Setup(s => s.GetAccessibleRoomsAsync(_testCharacterId, cancellationToken))
            .ThrowsAsync(new KeyNotFoundException($"Character not found: {_testCharacterId}"));

        // Act: 존재하지 않는 캐릭터로 조회
        var result = await _controller.GetRooms(cancellationToken);

        // Assert: 404 Not Found
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetRooms_ServiceException_Returns500InternalServerError()
    {
        // Arrange: Service에서 예기치 않은 예외 발생
        var cancellationToken = CancellationToken.None;

        _mockChatService
            .Setup(s => s.GetAccessibleRoomsAsync(_testCharacterId, cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act: Service 예외 발생
        var result = await _controller.GetRooms(cancellationToken);

        // Assert: 500 Internal Server Error
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task GetRooms_EmptyRoomList_Returns200WithEmptyList()
    {
        // Arrange: 접근 가능한 채팅방이 없음
        var cancellationToken = CancellationToken.None;

        var emptyRooms = new List<ChatRoomDto>();

        _mockChatService
            .Setup(s => s.GetAccessibleRoomsAsync(_testCharacterId, cancellationToken))
            .ReturnsAsync(emptyRooms);

        // Act: 빈 채팅방 목록 조회
        var result = await _controller.GetRooms(cancellationToken);

        // Assert: 200 OK, 빈 리스트 반환
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var rooms = okResult!.Value as List<ChatRoomDto>;
        rooms.Should().BeEmpty();
    }
}
