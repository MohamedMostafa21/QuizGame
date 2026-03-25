using System.ComponentModel.DataAnnotations;

namespace QuizGame.ViewModels
{
    public class RegisterViewModel
    {
        public string Username { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        [MinLength(6)]
        public string Password { get; set; }

        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
