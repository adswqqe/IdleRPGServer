using System.ComponentModel.DataAnnotations;
namespace IdleRPG.Application.DTOs.Auth
{
    /// <summary>
    /// 회원가입 요청 DTO
    /// Unity의 [Serializable] 클래스처럼 데이터 전송용
    /// </summary>
    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, 
                      ErrorMessage = "Username must be 3-50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", 
                           ErrorMessage = "Username can only contain letters, numbers, and underscore")]
        public string Username { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, 
                      ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }
        
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}