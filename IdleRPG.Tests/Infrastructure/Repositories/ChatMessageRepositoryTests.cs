using FluentAssertions;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using IdleRPG.Domain.ValueObjects;
using IdleRPG.Infrastructure.Data;
using IdleRPG.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IdleRPG.Tests.Infrastructure.Repositories;

/// <summary>
/// ChatMessageRepository 통합 테스트 (InMemory Database)
/// N+1 쿼리 방지 및 Cursor 페이징 검증
/// </summary>
public class ChatMessageRepositoryTests : IDisposable
{
    private readonly GameDBContext _context;
    private readonly ChatMessageRepository _repository;
    private readonly Guid _testRoomId;
    private readonly Guid _testCharacterId1;
    private readonly Guid _testCharacterId2;

    public ChatMessageRepositoryTests()
    {
        // InMemory Database 설정
        var options = new DbContextOptionsBuilder<GameDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // 각 테스트마다 독립적인 DB
            .EnableSensitiveDataLogging() // SQL 로깅 (디버깅용)
            .Options;

        _context = new GameDBContext(options);
        _repository = new ChatMessageRepository(_context);

        _testRoomId = Guid.NewGuid();
        _testCharacterId1 = Guid.NewGuid();
        _testCharacterId2 = Guid.NewGuid();

        // 테스트 데이터 Seed
        SeedTestData();
    }

    /// <summary>
    /// 테스트 데이터 초기화
    /// </summary>
    private void SeedTestData()
    {
        // Character 생성 (Stats 포함)
        var character1 = new Character
        {
            Id = _testCharacterId1,
            PlayerId = Guid.NewGuid(),
            Level = 10,
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

        var character2 = new Character
        {
            Id = _testCharacterId2,
            PlayerId = Guid.NewGuid(),
            Level = 15,
            Experience = 0,
            Gold = 2000,
            Crystal = 200,
            Stats = new CharacterStats(
                attack: 120,
                defense: 60,
                maxHealth: 600,
                critRate: 0.15f,
                critDamage: 1.6f,
                evasion: 0.06f,
                attackSpeed: 1.1f
            )
        };

        _context.Characters.AddRange(character1, character2);

        // ChatRoom 생성 (Global 타입)
        var room = new ChatRoom
        {
            Id = _testRoomId,
            Type = RoomType.Global,
            Name = "전체 채팅",
            CreatedAt = DateTime.UtcNow
        };

        _context.ChatRooms.Add(room);

        // ChatMessage 생성 (10개)
        var messages = new List<ChatMessage>();
        for (int i = 0; i < 10; i++)
        {
            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                RoomId = _testRoomId,
                SenderId = i % 2 == 0 ? _testCharacterId1 : _testCharacterId2, // 번갈아가며
                Content = $"Test message {i + 1}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i) // 최신순
            };
            messages.Add(message);
        }

        _context.ChatMessages.AddRange(messages);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    // ========== GetByRoomIdAsync Tests ==========

    [Fact]
    public async Task GetByRoomIdAsync_WithoutBeforeId_ReturnsLatestMessages()
    {
        // Arrange: beforeId 없이 최신 메시지 조회
        var take = 5;
        var cancellationToken = CancellationToken.None;

        // Act: Repository 호출
        var result = await _repository.GetByRoomIdAsync(_testRoomId, null, take, cancellationToken);

        // Assert: 최신 5개 메시지 반환
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        result.Should().BeInDescendingOrder(m => m.CreatedAt); // 최신순 정렬
        result[0].Content.Should().Be("Test message 1"); // 가장 최신
        result[4].Content.Should().Be("Test message 5");
    }

    [Fact]
    public async Task GetByRoomIdAsync_WithBeforeId_ReturnsPreviousMessages()
    {
        // Arrange: Cursor 페이징 (beforeId 제공)
        var take = 3;
        var cancellationToken = CancellationToken.None;

        // 먼저 최신 메시지 조회
        var latestMessages = await _repository.GetByRoomIdAsync(_testRoomId, null, 5, cancellationToken);
        var fifthMessage = latestMessages[4]; // 5번째 메시지

        // Act: 5번째 메시지 이전의 메시지 조회
        var result = await _repository.GetByRoomIdAsync(_testRoomId, fifthMessage.Id, take, cancellationToken);

        // Assert: 이전 3개 메시지 반환
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeInDescendingOrder(m => m.CreatedAt);
        result[0].Content.Should().Be("Test message 6");
        result[1].Content.Should().Be("Test message 7");
        result[2].Content.Should().Be("Test message 8");
    }

    [Fact]
    public async Task GetByRoomIdAsync_WithInvalidBeforeId_ReturnsAllMessages()
    {
        // Arrange: 존재하지 않는 beforeId
        var invalidBeforeId = Guid.NewGuid();
        var take = 5;
        var cancellationToken = CancellationToken.None;

        // Act: 존재하지 않는 beforeId로 조회
        var result = await _repository.GetByRoomIdAsync(_testRoomId, invalidBeforeId, take, cancellationToken);

        // Assert: beforeId가 무시되고 최신 메시지 반환
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        result[0].Content.Should().Be("Test message 1");
    }

    [Fact]
    public async Task GetByRoomIdAsync_EagerLoadsSender_NoN1Query()
    {
        // Arrange: N+1 쿼리 방지 검증
        var take = 5;
        var cancellationToken = CancellationToken.None;

        // Act: Repository 호출
        var result = await _repository.GetByRoomIdAsync(_testRoomId, null, take, cancellationToken);

        // Assert: Sender가 Eager Loading되어 있음 (추가 쿼리 없이 접근 가능)
        result.Should().NotBeNull();
        result.Should().HaveCount(5);

        foreach (var message in result)
        {
            // Sender 정보 접근 (N+1 쿼리 없이 가능)
            message.Sender.Should().NotBeNull();
            message.Sender.Level.Should().BeGreaterThan(0);

            // Character에 Stats가 있어야 함
            message.Sender.Stats.Should().NotBeNull();
            message.Sender.Stats.Attack.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task GetByRoomIdAsync_UsesAsNoTracking_EntitiesNotModified()
    {
        // Arrange: AsNoTracking 사용 확인
        var take = 5;
        var cancellationToken = CancellationToken.None;

        // Act: Repository 호출
        var result = await _repository.GetByRoomIdAsync(_testRoomId, null, take, cancellationToken);

        // Assert: 조회된 엔티티를 수정해도 DB에 반영되지 않음 (AsNoTracking)
        var firstMessage = result.First();
        firstMessage.Content = "Modified content";

        // SaveChanges를 호출해도 변경사항이 없음
        var changes = await _context.SaveChangesAsync(cancellationToken);
        changes.Should().Be(0, "AsNoTracking entities should not be tracked for changes");
    }

    [Fact]
    public async Task GetByRoomIdAsync_EmptyRoom_ReturnsEmptyList()
    {
        // Arrange: 메시지가 없는 방
        var emptyRoomId = Guid.NewGuid();
        var emptyRoom = new ChatRoom
        {
            Id = emptyRoomId,
            Type = RoomType.Global,
            Name = "Empty Room",
            CreatedAt = DateTime.UtcNow
        };
        _context.ChatRooms.Add(emptyRoom);
        await _context.SaveChangesAsync();

        var take = 5;
        var cancellationToken = CancellationToken.None;

        // Act: 빈 방 조회
        var result = await _repository.GetByRoomIdAsync(emptyRoomId, null, take, cancellationToken);

        // Assert: 빈 리스트 반환
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByRoomIdAsync_TakeLimitRespected_ReturnsExactCount()
    {
        // Arrange: take 제한 확인 (10개 중 3개만 조회)
        var take = 3;
        var cancellationToken = CancellationToken.None;

        // Act: take 제한으로 조회
        var result = await _repository.GetByRoomIdAsync(_testRoomId, null, take, cancellationToken);

        // Assert: 정확히 3개만 반환
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    // ========== GetByIdAsync Tests ==========

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsMessageWithSender()
    {
        // Arrange: 존재하는 메시지 ID
        var existingMessage = await _context.ChatMessages.FirstAsync();
        var cancellationToken = CancellationToken.None;

        // Act: ID로 메시지 조회
        var result = await _repository.GetByIdAsync(existingMessage.Id, cancellationToken);

        // Assert: 메시지와 Sender 정보 반환
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingMessage.Id);
        result.Sender.Should().NotBeNull();
        result.Sender.Level.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange: 존재하지 않는 메시지 ID
        var invalidId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        // Act: 존재하지 않는 ID로 조회
        var result = await _repository.GetByIdAsync(invalidId, cancellationToken);

        // Assert: null 반환
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_UsesAsNoTracking_EntityNotModified()
    {
        // Arrange: AsNoTracking 사용 확인
        var existingMessage = await _context.ChatMessages.FirstAsync();
        var cancellationToken = CancellationToken.None;

        // Act: Repository 호출
        var result = await _repository.GetByIdAsync(existingMessage.Id, cancellationToken);

        // Assert: 조회된 엔티티를 수정해도 DB에 반영되지 않음 (AsNoTracking)
        result!.Content = "Modified content by GetById";

        // SaveChanges를 호출해도 변경사항이 없음
        var changes = await _context.SaveChangesAsync(cancellationToken);
        changes.Should().Be(0, "AsNoTracking entities should not be tracked for changes");
    }

    // ========== AddAsync Tests ==========

    [Fact]
    public async Task AddAsync_ValidMessage_AddsToContext()
    {
        // Arrange: 새로운 메시지 생성
        var newMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            RoomId = _testRoomId,
            SenderId = _testCharacterId1,
            Content = "New test message",
            CreatedAt = DateTime.UtcNow
        };
        var cancellationToken = CancellationToken.None;

        // Act: 메시지 추가
        await _repository.AddAsync(newMessage, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Assert: 데이터베이스에 저장됨
        var savedMessage = await _context.ChatMessages
            .AsNoTracking()
            .SingleOrDefaultAsync(m => m.Id == newMessage.Id, cancellationToken);

        savedMessage.Should().NotBeNull();
        savedMessage!.Content.Should().Be("New test message");
    }
}
