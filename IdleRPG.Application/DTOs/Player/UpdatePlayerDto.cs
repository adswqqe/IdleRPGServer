using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IdleRPG.Application.DTOs.Player
{
    public class UpdatePlayerDto
    {
        // 클라이언트가 보내지 않음 - 서버에서 JWT 토큰으로 설정
        [JsonIgnore] // Swagger/JSON에서 숨김
        public Guid Id { get; set; }

        [Required(ErrorMessage = "UserName is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "UserName must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "UserName can only contain letters, numbers, and underscores")]
        public string UserName { get; set; }
    }
}