using System.ComponentModel.DataAnnotations;
namespace IdleRPG.Application.DTOs.Auth
{
    /// <summary>
    /// 로그인 요청 DTO
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}