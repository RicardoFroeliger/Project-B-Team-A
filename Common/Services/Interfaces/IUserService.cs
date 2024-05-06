using Common.DAL.Models;
using Common.Enums;

namespace Common.Services.Interfaces
{
    public interface IUserService : IBaseService<User>
    {
        (bool Valid, string Message) ValidateUserpass(int userpass);
        (bool Valid, string Message) ValidateUserForRole(int userpass, Role allowedRole);
        (bool Valid, string Message) ValidateUserForRole(User? user, Role allowedRole);
        List<User> GetUsersOfRole(Role role);
    }
}
