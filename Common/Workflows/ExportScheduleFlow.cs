using Common.DAL.Interfaces;
using Common.Services.Interfaces;

namespace Common.Workflows
{
    public class ExportScheduleFlow : Workflow
    {
        private IUserService UserService { get; }

        public ExportScheduleFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, IUserService userService)
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
