﻿using Common.DAL;
using Common.Enums;
using Common.Services;

namespace Common.Workflows.Guide
{
    public class StartTourGuideFlow : TourGuideFlow
    {
        private ISettingsService SettingsService { get; }
        private IUserService UserService { get; }
        public FlowStep Step { get; set; } = FlowStep.ScanRegistration;
        public int GuideId { get; private set; }

        public StartTourGuideFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, ITourService tourService, ISettingsService settingsService, IUserService userService)
            : base(context, localizationService, ticketService, tourService)
        {
            SettingsService = settingsService;
            UserService = userService;
        }

        public (bool Success, string Message) AddScannedTicket(int ticketNumber, bool extra = false)
        {
            var validation = ValidateTicket(ticketNumber);
            if (!validation.Success)
                return validation;

            if (!Tour!.RegisteredTickets.Contains(ticketNumber) && !extra)
                return (false, Localization.Get("Flow_ticket_not_in_tour"));

            if (TicketBuffer.Keys.ToList().Contains(ticketNumber))
                return (false, Localization.Get("Flow_ticket_already_added_to_list"));

            if (TicketBuffer.Count >= SettingsService.GetValueAsInt("Max_capacity_per_tour")!.Value)
                return (false, Localization.Get("Flow_tour_no_space_for_tickets_in_tour"));

            TicketBuffer.Add(ticketNumber, extra);

            return (true, Localization.Get("Flow_ticket_added_to_list"));
        }

        public (bool Success, string Message) ScanBadge(int userId)
        {
            var validation = UserService.ValidateUserpass(userId);
            if (!validation.Valid)
                return validation;

            GuideId = userId;
            ProgressStep();

            return (true, Localization.Get("Flow_next_step"));
        }

        public void ProgressStep()
        {
            switch (Step)
            {
                case FlowStep.ScanRegistration:
                    Step = FlowStep.ScanExtra;
                    break;
                case FlowStep.ScanExtra:
                    Step = FlowStep.Finalize;
                    break;
            }
        }

        public override (bool Succeeded, string Message) Commit()
        {
            if (!TicketBuffer.Any())
                return (false, Localization.Get("Flow_no_tickets_scanned"));

            Tour!.RegisteredTickets = TicketBuffer.Keys.ToList();
            Tour!.GuideId = GuideId;
            Tour!.Departed = true;

            return base.Commit();
        }
    }
}
