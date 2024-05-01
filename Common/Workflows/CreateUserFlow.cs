using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Enums;
using Common.Services.Interfaces;

namespace Common.Workflows
{
    public class CreateUserFlow : Workflow
    {
        private IUserService UserService { get; }
        private string Username { get; set; } = "";
        private Role? Role { get; set; }

        public CreateUserFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, IUserService userService)
            : base(context, localizationService, ticketService)
        {
            UserService = userService;
        }

        public (bool Succeeded, string Message) SetUsername(string username)
        {
            Username = username;

            return (true, Localization.Get("Flow_username_set"));
        }

        public (bool Succeeded, string Message) SetRole(Role role)
        {
            Role = role;

            return (true, Localization.Get("Flow_role_set"));
        }

        public override (bool Succeeded, string Message) Commit()
        {
            if (string.IsNullOrWhiteSpace(Username))
                return (false, Localization.Get("Flow_username_not_set"));

            if (Role == null)
                return (false, Localization.Get("Flow_role_not_set"));

            UserService.AddOne(new User() { Name = Username, Role = (int)Role.Value });

            return base.Commit();
        }
    }
}
