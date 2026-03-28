using System.ComponentModel.DataAnnotations;

namespace QuizGame.ViewModels
{
    public class JoinRoomViewModel
    {

        [Required(ErrorMessage = "Room code is required.")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Room code must be exactly 5 characters.")]
        [RegularExpression("^[A-Z0-9]*$", ErrorMessage = "Room code must be uppercase letters and numbers only.")]
        [Display(Name = "Room Code")]
        public string RoomCode { get; set; }
    }
}
