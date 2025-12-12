using FluentAssertions;
using IdleRPG.Application.DTOs.Chat;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using IdleRPG.Domain.Repositories;
using IdleRPG.Domain.ValueObjects;
using IdleRPG.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IdleRPG.Tests.Application.Services;

/// <summary>
/// ChatService 단위 테스트
/// </summary>
public class ChatServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IChatRoomRepository> _mockChatRoomRepository;
    private readonly Mock<IChatMessageRepository> _mockChatMessageRepository;
    private readonly Mock<ICharacterRepository> _mockCharacterRepository;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<ChatService>> _mockLogger;
    private readonly ChatService _chatService;

    public ChatServiceTests()
    {
        // Arrange: Mock 객체 초기화
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockChatRoomRepository = new Mock<IChatRoomRepository>();
        _mockChatMessageRepository = new Mock<IChatMessageRepository>();
        _mockCharacterRepository = new Mock<ICharacterRepository>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _mockLogger = new Mock<ILogger<ChatService>>();

        // UnitOfWork의 Repository 속성 설정
        _mockUnitOfWork.Setup(u => u.ChatRooms).Returns(_mockChatRoomRepository.Object);
        _mockUnitOfWork.Setup(u => u.ChatMessages).Returns(_mockChatMessageRepository.Object);
        _mockUnitOfWork.Setup(u => u.Characters).Returns(_mockCharacterRepository.Object);

        // ChatService 인스턴스 생성
        _chatService = new ChatService(_mockUnitOfWork.Object, _cache, _mockLogger.Object);
    }

    // ========== Helper Methods ==========

    /// <summary>
    /// 테스트용 Character 엔티티 생성
    /// </summary>
    private Character CreateTestCharacter(Guid? id = null, int level = 10)
    {
        return new Character
        {
            Id = id ?? Guid.NewGuid(),
            PlayerId = Guid.NewGuid(),
            Level = level,
            Experience = 0,
            Gold = 1000,
            Crystal = 100,
            Stats = new CharacterStats(
                attack: 100,
                defense: 50,
                maxHealth: 500,
                critRate: 0.1f,
                critDamage: 1.5f,
                evasion: 0.05f,
                attackSpeed: 1.0f
            )
        };
    }

    // ========== SendMessageAsync Tests ==========

    [Fact]
    public async Task SendMessageAsync_ValidMessage_ReturnsChatMessageDto()
    {
        // Arrange: 정상 메시지 전송 시나리오
        var roomId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var content = "Hello, World!";
        var cancellationToken = CancellationToken.None;

        // 채팅방 Mock (Global 타입으로 권한 체크 통과)
        var room = new ChatRoom
        {
            Id = roomId,
            Type = RoomType.Global,
            Name = "전체 채팅",
            CreatedAt = DateTime.UtcNow
        };

        _mockChatRoomRepository
            .Setup(r => r.GetByIdAsync(roomId, cancellationToken))
            .ReturnsAsync(room);

        // 메시지 저장 후 다시 조회할 때 사용할 Mock
        var savedMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            SenderId = senderId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            Sender = CreateTestCharacter(senderId, 10)
        };

        _mockChatMessageRepository
            .Setup(r => r.AddAsync(It.IsAny<ChatMessage>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        _mockChatMessageRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), cancellationToken))
            .ReturnsAsync(savedMessage);

        // Act: 메시지 전송
        var result = await _chatService.SendMessageAsync(roomId, senderId, content, cancellationToken);

        // Assert: 결과 검증
        result.Should().NotBeNull();
        result.RoomId.Should().Be(roomId);
        result.Sender.Should().NotBeNull();
        result.Sender.Id.Should().Be(senderId);
        result.Sender.Level.Should().Be(10);
        result.Content.Should().Be(content);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Mock 호출 검증
        _mockChatRoomRepository.Verify(r => r.GetByIdAsync(roomId, cancellationToken), Times.Once);
        _mockChatMessageRepository.Verify(r => r.AddAsync(It.IsAny<ChatMessage>(), cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_EmptyMessage_ThrowsArgumentException()
    {
        // Arrange: 빈 메시지 전송 시나리오
        var roomId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var content = "";
        var cancellationToken = CancellationToken.None;

        // Act & Assert: 빈 메시지로 예외 발생
        var act = async () => await _chatService.SendMessageAsync(roomId, senderId, content, cancellationToken);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*비워둘 수 없습니다*");
    }

    [Fact]
    public async Task SendMessageAsync_MessageTooLong_ThrowsArgumentException()
    {
        // Arrange: 길이 초과 메시지 (501자)
        var roomId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var content = new string('A', 501); // 501자
        var cancellationToken = CancellationToken.None;

        // Act & Assert: 길이 초과로 예외 발생
        var act = async () => await _chatService.SendMessageAsync(roomId, senderId, content, cancellationToken);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*최대 500자*");
    }

    [Fact]
    public async Task SendMessageAsync_CooldownViolation_ThrowsInvalidOperationException()
    {
        // Arrange: 쿨다운 위반 시나리오 (1초 이내 재전송)
        var roomId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var content = "First message";
        var cancellationToken = CancellationToken.None;

        // 채팅방 Mock (Global 타입)
        var room = new ChatRoom
        {
            Id = roomId,
            Type = RoomType.Global,
            Name = "전체 채팅",
            CreatedAt = DateTime.UtcNow
        };

        _mockChatRoomRepository
            .Setup(r => r.GetByIdAsync(roomId, cancellationToken))
            .ReturnsAsync(room);

        var savedMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            SenderId = senderId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            Sender = CreateTestCharacter(senderId, 10)
        };

        _mockChatMessageRepository
            .Setup(r => r.AddAsync(It.IsAny<ChatMessage>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        _mockChatMessageRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), cancellationToken))
            .ReturnsAsync(savedMessage);

        // 첫 번째 메시지 전송 성공
        await _chatService.SendMessageAsync(roomId, senderId, content, cancellationToken);

        // Act & Assert: 쿨다운 위반 (1초 이내 재전송)
        var act = async () => await _chatService.SendMessageAsync(roomId, senderId, "Second message", cancellationToken);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*너무 빠르게 전송*");
    }

    [Fact]
    public async Task SendMessageAsync_RoomNotFound_ThrowsUnauthorizedAccessException()
    {
        // Arrange: 존재하지 않는 채팅방
        var roomId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var content = "Test message";
        var cancellationToken = CancellationToken.None;

        // 채팅방이 존재하지 않음
        _mockChatRoomRepository
            .Setup(r => r.GetByIdAsync(roomId, cancellationToken))
            .ReturnsAsync((ChatRoom?)null);

        // Act & Assert: 채팅방 미존재로 권한 없음 예외
        var act = async () => await _chatService.SendMessageAsync(roomId, senderId, content, cancellationToken);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*권한이 없습니다*");
    }

    // ========== GetMessagesAsync Tests ==========

    [Fact]
    public async Task GetMessagesAsync_WithoutBeforeId_ReturnsLatest50Messages()
    {
        // Arrange: beforeId 없이 최신 50개 조회
        var roomId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        // 채팅방 Mock (Global 타입)
        var room = new ChatRoom
        {
            Id = roomId,
            Type = RoomType.Global,
            Name = "전체 채팅",
            CreatedAt = DateTime.UtcNow
        };

        _mockChatRoomRepository
            .Setup(r => r.GetByIdAsync(roomId, cancellationToken))
            .ReturnsAsync(room);

        // 메시지 목록 Mock (50개)
        var testCharacter = CreateTestCharacter(characterId, 10);
        var messages = Enumerable.Range(1, 50).Select(i => new ChatMessage
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            SenderId = characterId,
            Content = $"Message {i}",
            CreatedAt = DateTime.UtcNow.AddMinutes(-i),
            Sender = testCharacter
        }).ToList();

        _mockChatMessageRepository
            .Setup(r => r.GetByRoomIdAsync(roomId, null, 50, cancellationToken))
            .ReturnsAsync(messages);

        // Act: 메시지 조회
        var result = await _chatService.GetMessagesAsync(roomId, characterId, null, 50, cancellationToken);

        // Assert: 결과 검증
        result.Should().NotBeNull();
        result.Should().HaveCount(50);
        result.First().Content.Should().Be("Message 1");

        // Mock 호출 검증
        _mockChatMessageRepository.Verify(r => r.GetByRoomIdAsync(roomId, null, 50, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetMessagesAsync_WithBeforeId_ReturnsPreviousMessages()
    {
        // Arrange: beforeId 제공하여 이전 메시지 조회
        var roomId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var beforeId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        // 채팅방 Mock (Global 타입)
        var room = new ChatRoom
        {
            Id = roomId,
            Type = RoomType.Global,
            Name = "전체 채팅",
            CreatedAt = DateTime.UtcNow
        };

        _mockChatRoomRepository
            .Setup(r => r.GetByIdAsync(roomId, cancellationToken))
            .ReturnsAsync(room);

        // 메시지 목록 Mock (이전 50개)
        var testCharacter = CreateTestCharacter(characterId, 10);
        var messages = Enumerable.Range(51, 50).Select(i => new ChatMessage
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            SenderId = characterId,
            Content = $"Message {i}",
            CreatedAt = DateTime.UtcNow.AddMinutes(-i),
            Sender = testCharacter
        }).ToList();

        _mockChatMessageRepository
            .Setup(r => r.GetByRoomIdAsync(roomId, beforeId, 50, cancellationToken))
            .ReturnsAsync(messages);

        // Act: 메시지 조회
        var result = await _chatService.GetMessagesAsync(roomId, characterId, beforeId, 50, cancellationToken);

        // Assert: 결과 검증
        result.Should().NotBeNull();
        result.Should().HaveCount(50);
        result.First().Content.Should().Be("Message 51");

        // Mock 호출 검증
        _mockChatMessageRepository.Verify(r => r.GetByRoomIdAsync(roomId, beforeId, 50, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetMessagesAsync_NoAccess_ThrowsUnauthorizedAccessException()
    {
        // Arrange: 권한 없는 채팅방 접근
        var roomId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        // Whisper 채팅방 (참여자가 아님)
        var room = new ChatRoom
        {
            Id = roomId,
            Type = RoomType.Whisper,
            Name = "Whisper Room",
            CreatedAt = DateTime.UtcNow,
            Participants = new List<ChatRoomParticipant>() // 빈 참여자 목록
        };

        _mockChatRoomRepository
            .Setup(r => r.GetByIdAsync(roomId, cancellationToken))
            .ReturnsAsync(room);

        // Act & Assert: 권한 없음 예외
        var act = async () => await _chatService.GetMessagesAsync(roomId, characterId, null, 50, cancellationToken);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*권한이 없습니다*");
    }

    // ========== CanAccessRoomAsync Tests ==========

    [Fact]
    public async Task CanAccessRoomAsync_GlobalRoom_ReturnsTrue()
    {
        // Arrange: Global 채팅방 접근
        var roomId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var room = new ChatRoom
        {
            Id = roomId,
            Type = RoomType.Global,
            Name = "전체 채팅",
            CreatedAt = DateTime.UtcNow
        };

        _mockChatRoomRepository
            .Setup(r => r.GetByIdAsync(roomId, cancellationToken))
            .ReturnsAsync(room);

        // Act: 권한 체크
        var result = await _chatService.CanAccessRoomAsync(characterId, roomId, cancellationToken);

        // Assert: Global 채팅방은 누구나 접근 가능
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessRoomAsync_GuildRoom_ReturnsFalse()
    {
        // Arrange: Guild 채팅방 접근 (Guild 시스템 미구현)
        var roomId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var guildId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var room = new ChatRoom
        {
            Id = roomId,
            Type = RoomType.Guild,
            Name = "길드 채팅",
            GuildId = guildId,
            CreatedAt = DateTime.UtcNow
        };

        _mockChatRoomRepository
            .Setup(r => r.GetByIdAsync(roomId, cancellationToken))
            .ReturnsAsync(room);

        // Act: 권한 체크
        var result = await _chatService.CanAccessRoomAsync(characterId, roomId, cancellationToken);

        // Assert: Guild 시스템 미구현으로 false 반환
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanAccessRoomAsync_WhisperRoom_Participant_ReturnsTrue()
    {
        // Arrange: Whisper 채팅방 접근 (참여자)
        var roomId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var room = new ChatRoom
        {
            Id = roomId,
            Type = RoomType.Whisper,
            Name = "Whisper Room",
            CreatedAt = DateTime.UtcNow,
            Participants = new List<ChatRoomParticipant>
            {
                new ChatRoomParticipant
                {
                    Id = Guid.NewGuid(),
                    RoomId = roomId,
                    CharacterId = characterId, // 참여자
                    JoinedAt = DateTime.UtcNow
                }
            }
        };

        _mockChatRoomRepository
            .Setup(r => r.GetByIdAsync(roomId, cancellationToken))
            .ReturnsAsync(room);

        // Act: 권한 체크
        var result = await _chatService.CanAccessRoomAsync(characterId, roomId, cancellationToken);

        // Assert: 참여자이므로 true 반환
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessRoomAsync_WhisperRoom_NonParticipant_ReturnsFalse()
    {
        // Arrange: Whisper 채팅방 접근 (비참여자)
        var roomId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var otherCharacterId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var room = new ChatRoom
        {
            Id = roomId,
            Type = RoomType.Whisper,
            Name = "Whisper Room",
            CreatedAt = DateTime.UtcNow,
            Participants = new List<ChatRoomParticipant>
            {
                new ChatRoomParticipant
                {
                    Id = Guid.NewGuid(),
                    RoomId = roomId,
                    CharacterId = otherCharacterId, // 다른 캐릭터
                    JoinedAt = DateTime.UtcNow
                }
            }
        };

        _mockChatRoomRepository
            .Setup(r => r.GetByIdAsync(roomId, cancellationToken))
            .ReturnsAsync(room);

        // Act: 권한 체크
        var result = await _chatService.CanAccessRoomAsync(characterId, roomId, cancellationToken);

        // Assert: 비참여자이므로 false 반환
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanAccessRoomAsync_RoomNotFound_ReturnsFalse()
    {
        // Arrange: 존재하지 않는 채팅방
        var roomId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        _mockChatRoomRepository
            .Setup(r => r.GetByIdAsync(roomId, cancellationToken))
            .ReturnsAsync((ChatRoom?)null);

        // Act: 권한 체크
        var result = await _chatService.CanAccessRoomAsync(characterId, roomId, cancellationToken);

        // Assert: 채팅방 미존재로 false 반환
        result.Should().BeFalse();
    }

    // ========== CreateWhisperRoomAsync Tests ==========

    [Fact]
    public async Task CreateWhisperRoomAsync_NewRoom_ReturnsNewRoomId()
    {
        // Arrange: 새로운 Whisper 방 생성
        var characterAId = Guid.NewGuid();
        var characterBId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var characterA = CreateTestCharacter(characterAId, 10);
        var characterB = CreateTestCharacter(characterBId, 15);

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterAId))
            .ReturnsAsync(characterA);

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterBId))
            .ReturnsAsync(characterB);

        // 기존 방 없음
        _mockChatRoomRepository
            .Setup(r => r.GetAccessibleRoomsAsync(characterAId, null, cancellationToken))
            .ReturnsAsync(new List<ChatRoom>());

        _mockChatRoomRepository
            .Setup(r => r.AddAsync(It.IsAny<ChatRoom>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        // Act: Whisper 방 생성
        var result = await _chatService.CreateWhisperRoomAsync(characterAId, characterBId, cancellationToken);

        // Assert: 새 방 ID 반환
        result.Should().NotBeEmpty();

        // Mock 호출 검증
        _mockChatRoomRepository.Verify(r => r.AddAsync(It.IsAny<ChatRoom>(), cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreateWhisperRoomAsync_ExistingRoom_ReturnsExistingRoomId()
    {
        // Arrange: 기존 Whisper 방 재사용
        var characterAId = Guid.NewGuid();
        var characterBId = Guid.NewGuid();
        var existingRoomId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var characterA = CreateTestCharacter(characterAId, 10);
        var characterB = CreateTestCharacter(characterBId, 15);

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterAId))
            .ReturnsAsync(characterA);

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterBId))
            .ReturnsAsync(characterB);

        // 기존 Whisper 방 존재
        var existingRoom = new ChatRoom
        {
            Id = existingRoomId,
            Type = RoomType.Whisper,
            Name = "Whisper Room",
            CreatedAt = DateTime.UtcNow,
            Participants = new List<ChatRoomParticipant>
            {
                new ChatRoomParticipant
                {
                    Id = Guid.NewGuid(),
                    RoomId = existingRoomId,
                    CharacterId = characterAId,
                    JoinedAt = DateTime.UtcNow
                },
                new ChatRoomParticipant
                {
                    Id = Guid.NewGuid(),
                    RoomId = existingRoomId,
                    CharacterId = characterBId,
                    JoinedAt = DateTime.UtcNow
                }
            }
        };

        _mockChatRoomRepository
            .Setup(r => r.GetAccessibleRoomsAsync(characterAId, null, cancellationToken))
            .ReturnsAsync(new List<ChatRoom> { existingRoom });

        // Act: Whisper 방 생성 시도 (기존 방 재사용)
        var result = await _chatService.CreateWhisperRoomAsync(characterAId, characterBId, cancellationToken);

        // Assert: 기존 방 ID 반환
        result.Should().Be(existingRoomId);

        // Mock 호출 검증 (새 방 생성하지 않음)
        _mockChatRoomRepository.Verify(r => r.AddAsync(It.IsAny<ChatRoom>(), cancellationToken), Times.Never);
    }

    [Fact]
    public async Task CreateWhisperRoomAsync_SameCharacter_ThrowsInvalidOperationException()
    {
        // Arrange: 자기 자신과 Whisper
        var characterId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        // Act & Assert: 자기 자신과 Whisper로 예외 발생
        var act = async () => await _chatService.CreateWhisperRoomAsync(characterId, characterId, cancellationToken);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*자기 자신*");
    }

    [Fact]
    public async Task CreateWhisperRoomAsync_CharacterANotFound_ThrowsKeyNotFoundException()
    {
        // Arrange: 캐릭터 A 미존재
        var characterAId = Guid.NewGuid();
        var characterBId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterAId))
            .ReturnsAsync((Character?)null);

        // Act & Assert: 캐릭터 A 미존재로 예외 발생
        var act = async () => await _chatService.CreateWhisperRoomAsync(characterAId, characterBId, cancellationToken);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{characterAId}*");
    }

    [Fact]
    public async Task CreateWhisperRoomAsync_CharacterBNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange: 캐릭터 B 미존재
        var characterAId = Guid.NewGuid();
        var characterBId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var characterA = CreateTestCharacter(characterAId, 10);

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterAId))
            .ReturnsAsync(characterA);

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterBId))
            .ReturnsAsync((Character?)null);

        // Act & Assert: 캐릭터 B 미존재로 예외 발생
        var act = async () => await _chatService.CreateWhisperRoomAsync(characterAId, characterBId, cancellationToken);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{characterBId}*");
    }

    // ========== GetAccessibleRoomsAsync Tests ==========

    [Fact]
    public async Task GetAccessibleRoomsAsync_ValidCharacter_ReturnsRoomList()
    {
        // Arrange: 정상 캐릭터로 채팅방 목록 조회
        var characterId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var character = CreateTestCharacter(characterId, 10);

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync(character);

        // 접근 가능한 채팅방 목록 Mock
        var rooms = new List<ChatRoom>
        {
            new ChatRoom
            {
                Id = Guid.NewGuid(),
                Type = RoomType.Global,
                Name = "전체 채팅",
                CreatedAt = DateTime.UtcNow
            },
            new ChatRoom
            {
                Id = Guid.NewGuid(),
                Type = RoomType.Whisper,
                Name = "Whisper Room",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockChatRoomRepository
            .Setup(r => r.GetAccessibleRoomsAsync(characterId, null, cancellationToken))
            .ReturnsAsync(rooms);

        // Act: 채팅방 목록 조회
        var result = await _chatService.GetAccessibleRoomsAsync(characterId, cancellationToken);

        // Assert: 결과 검증
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Type.Should().Be("Global");
        result[1].Type.Should().Be("Whisper");
    }

    [Fact]
    public async Task GetAccessibleRoomsAsync_CharacterNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange: 존재하지 않는 캐릭터
        var characterId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        _mockCharacterRepository
            .Setup(r => r.GetByIdAsync(characterId))
            .ReturnsAsync((Character?)null);

        // Act & Assert: 캐릭터 미존재로 예외 발생
        var act = async () => await _chatService.GetAccessibleRoomsAsync(characterId, cancellationToken);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{characterId}*");
    }
}
