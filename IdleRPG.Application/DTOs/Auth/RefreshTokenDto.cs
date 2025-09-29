using System.ComponentModel.DataAnnotations;
namespace IdleRPG.Application.DTOs.Auth
{
    /// <summary>
    /// Refresh Token 요청 DTO
    /// </summary>
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}