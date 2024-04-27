using Common.DAL;
using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Enums;
using Common.Services;
using Common.Services.Interfaces;

namespace Common.Workflows
{
    public class CreateUserFlow : Workflow
    {
        private IUserService UserService { get; }
        private string Username { get; set; }
        private Role Role { get; set; }

        public CreateUserFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, IUserService userService)
            : base(context, localizationService, ticketService)
        {
            UserService = userService;
        }

        public (bool Succeeded, string Message) SetUsername(string username)
        {
            Username = username;

            return (true, "");
        }

        public (bool Succeeded, string Message) SetRole(Role role)
        {
            Role = role;

            return (true, "");
        }

        public override (bool Succeeded, string Message) Commit()
        {
            UserService.AddUser(new User() { Name = Username, Role = (int)Role });

            return base.Commit();
        }
    }
}
