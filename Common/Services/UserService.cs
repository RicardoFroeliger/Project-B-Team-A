using Common.DAL;
using Common.DAL.Models;
using Common.Enums;

namespace Common.Services
{
    public class UserService : BaseService<User>, IUserService
    {
        public ISettingsService Settings { get; }
        private ILocalizationService Localization { get; }

        public UserService(IDepotContext context, ISettingsService settings, ILocalizationService localization)
            : base(context)
        {
            Localization = localization;
            Settings = settings;
        }

        public (bool Valid, string Message) ValidateUserpass(int userpass)
        {
            var user = Table.FirstOrDefault(user => user.Id == userpass);

            if (user == null)
                return new(false, Localization.Get("User_does_not_exist"));

            return new(true, Localization.Get("User_is_valid"));
        }

        public (bool Valid, string Message) ValidateUserForRole(int userpass, RoleType allowedRole) => ValidateUserForRole(GetOne(userpass), allowedRole);

        public (bool Valid, string Message) ValidateUserForRole(User? user, RoleType allowedRole)
        {
            if (user == null)
                return new(false, Localization.Get("User_does_not_exist"));

            if (user.Role < (int)allowedRole)
                return new(false, Localization.Get("User_has_no_access"));

            return new(true, Localization.Get("User_has_access"));
        }

        public List<User> GetUsersOfRole(RoleType role)
        {
            return Table.Where(user => user.Role == (int)role).ToList();
        }

        public override User AddOne(User user)
        {
            user.Id = Table.OrderByDescending(u => u.Id).First().Id + 100;
            return base.AddOne(user);
        }

        public (bool Valid, string Message) ValidateUserForClient(int userId, ClientType clientType) => ValidateUserForClient(GetOne(userId), clientType);

        public (bool Valid, string Message) ValidateUserForClient(User? user, ClientType clientType)
        {
            var minimumRole = clientType switch
            {
                ClientType.Manager => RoleType.Manager,
                ClientType.Guide => RoleType.Guide,
                _ => RoleType.Guest
            };

            if (user == null)
                return new(false, Localization.Get("User_does_not_exist"));

            if (user.Role < (int)minimumRole)
                return new(false, Localization.Get("User_has_no_access"));

            return new(true, Localization.Get("User_has_access"));
        }
    }
}
