using QuizGame.Models;
using QuizGame.ViewModels;

namespace QuizGame.Services
{
    public class UserService
    {
        public User GetUserFromRegisterViewModel(RegisterViewModel vm)
        {
            return new User
            {
                UserName = vm.Username,
                Email = vm.Email
            };
        }
    }
}
