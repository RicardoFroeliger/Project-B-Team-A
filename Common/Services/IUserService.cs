using Common.DAL.Models;
using Common.Enums;

namespace Common.Services
{
    public interface IUserService : IBaseService<User>
    {
        (bool Valid, string Message) ValidateUserpass(int userId);
        (bool Valid, string Message) ValidateUserForClient(int userId, ClientType clientType);
        (bool Valid, string Message) ValidateUserForClient(User? user, ClientType clientType);
        (bool Valid, string Message) ValidateUserForRole(int userId, RoleType allowedRole);
        (bool Valid, string Message) ValidateUserForRole(User? user, RoleType allowedRole);
        List<User> GetUsersOfRole(RoleType role);
    }
}
