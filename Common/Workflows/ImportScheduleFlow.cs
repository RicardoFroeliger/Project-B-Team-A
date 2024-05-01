using Common.DAL.Interfaces;
using Common.Services.Interfaces;

namespace Common.Workflows
{
    public class ImportScheduleFlow : Workflow
    {
        private IUserService UserService { get; }

        public ImportScheduleFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, IUserService userService)
            : base(context, localizationService, ticketService)
        {
            UserService = userService;
        }

        public override (bool Succeeded, string Message) Commit()
        {
            return base.Commit();
        }
    }
}
