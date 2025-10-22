using FluentAssertions;
using IdleRPG.API.Controllers;
using IdleRPG.Application.DTOs.Gacha;
using IdleRPG.Application.Services;
using IdleRPG.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IdleRPG.Tests.API.Controllers
{
    /// <summary>
    /// SkillController 테스트
    /// API 레이어: HTTP 응답, 에러 처리, 로깅
    /// </summary>
    public class SkillControllerTests
    {
        private readonly Mock<ISkillService> _mockSkillService;
        private readonly Mock<ILogger<SkillController>> _mockLogger;
        private readonly SkillController _controller;

        public SkillControllerTests()
        {
            _mockSkillService = new Mock<ISkillService>();
            _mockLogger = new Mock<ILogger<SkillController>>();
            _controller = new SkillController(_mockSkillService.Object, _mockLogger.Object);
        }

        #region PerformGacha - Success Cases

        [Fact]
        public async Task PerformGacha_ValidRequest_Returns200WithSkillDto()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var request = new GachaRequest { CharacterId = characterId };

            var expectedSkill = new SkillDto
            {
                Id = 18,
                Name = "메테오",
                Description = "하늘에서 거대한 운석을 떨어뜨려 막대한 피해를 입힙니다.",
                Rarity = SkillRarity.Epic,
                Type = SkillType.Active
            };

            _mockSkillService.Setup(s => s.PerformGachaAsync(characterId, 100))
                .ReturnsAsync(expectedSkill);

            // Act
            var result = await _controller.PerformGacha(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedSkill);
        }

        [Fact]
        public async Task PerformGacha_LegendarySkill_Returns200()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var request = new GachaRequest { CharacterId = characterId };

            var legendarySkill = new SkillDto
            {
                Id = 22,
                Name = "신의 심판",
                Description = "신성한 빛으로 모든 적을 심판하여 즉사시킬 확률이 있습니다.",
                Rarity = SkillRarity.Legendary,
                Type = SkillType.Active
            };

            _mockSkillService.Setup(s => s.PerformGachaAsync(characterId, It.IsAny<int>()))
                .ReturnsAsync(legendarySkill);

            // Act
            var result = await _controller.PerformGacha(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var skill = okResult!.Value as SkillDto;
            skill!.Rarity.Should().Be(SkillRarity.Legendary);
        }

        #endregion

        #region PerformGacha - Error Cases

        [Fact]
        public async Task PerformGacha_InsufficientCrystal_Returns400BadRequest()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var request = new GachaRequest { CharacterId = characterId };

            _mockSkillService.Setup(s => s.PerformGachaAsync(characterId, It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Crystal이 부족합니다. (필요: 100, 보유: 50)"));

            // Act
            var result = await _controller.PerformGacha(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new
            {
                Message = "Crystal이 부족합니다. (필요: 100, 보유: 50)"
            });
        }

        [Fact]
        public async Task PerformGacha_CharacterNotFound_Returns400BadRequest()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var request = new GachaRequest { CharacterId = characterId };

            _mockSkillService.Setup(s => s.PerformGachaAsync(characterId, It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException($"캐릭터를 찾을 수 없습니다. (ID: {characterId})"));

            // Act
            var result = await _controller.PerformGacha(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new
            {
                Message = $"캐릭터를 찾을 수 없습니다. (ID: {characterId})"
            });
        }

        [Fact]
        public async Task PerformGacha_UnexpectedError_Returns500InternalServerError()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var request = new GachaRequest { CharacterId = characterId };

            _mockSkillService.Setup(s => s.PerformGachaAsync(characterId, It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.PerformGacha(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new
            {
                Message = "서버 오류가 발생했습니다."
            });
        }

        #endregion

        #region Logging Tests

        [Fact]
        public async Task PerformGacha_Success_LogsInformation()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var request = new GachaRequest { CharacterId = characterId };

            var skill = new SkillDto
            {
                Id = 5,
                Name = "치유",
                Rarity = SkillRarity.Common,
                Type = SkillType.Active
            };

            _mockSkillService.Setup(s => s.PerformGachaAsync(characterId, It.IsAny<int>()))
                .ReturnsAsync(skill);

            // Act
            await _controller.PerformGacha(request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("스킬 가챠 성공")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task PerformGacha_InvalidOperation_LogsWarning()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var request = new GachaRequest { CharacterId = characterId };

            _mockSkillService.Setup(s => s.PerformGachaAsync(characterId, It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Crystal이 부족합니다."));

            // Act
            await _controller.PerformGacha(request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("스킬 가챠 실패")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task PerformGacha_UnexpectedError_LogsError()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var request = new GachaRequest { CharacterId = characterId };

            var exception = new Exception("Unexpected error");
            _mockSkillService.Setup(s => s.PerformGachaAsync(characterId, It.IsAny<int>()))
                .ThrowsAsync(exception);

            // Act
            await _controller.PerformGacha(request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("스킬 가챠 처리 중 오류 발생")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region Service Interaction Tests

        [Fact]
        public async Task PerformGacha_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var request = new GachaRequest { CharacterId = characterId };

            var skill = new SkillDto
            {
                Id = 1,
                Name = "화염구",
                Rarity = SkillRarity.Common,
                Type = SkillType.Active
            };

            _mockSkillService.Setup(s => s.PerformGachaAsync(characterId, 100))
                .ReturnsAsync(skill);

            // Act
            await _controller.PerformGacha(request);

            // Assert
            _mockSkillService.Verify(s => s.PerformGachaAsync(characterId, 100), Times.Once);
        }

        #endregion
    }
}
