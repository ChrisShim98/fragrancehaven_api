using System.ComponentModel.DataAnnotations;

namespace fragrancehaven_api.DTOs
{
    public class PasswordResetDTO
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string CurrentPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
    }
}