using Common.DAL.Models;
using Common.Enums;

namespace Common.Services.Interfaces
{
    public interface IUserService
    {
        (bool Valid, string Message) ValidateUserpass(int userpass);
        (bool Valid, string Message) ValidateUserForRole(int userpass, Role allowedRole);
        (bool Valid, string Message) ValidateUserForRole(User? user, Role allowedRole);
        User? GetUser(int userpass);
        List<User> GetAllUsers();
        List<User> GetUsersOfRole(Role role);
        void AddUser(User user);
    }
}
