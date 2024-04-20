﻿using Common.DAL;
using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Services;
using Common.Services.Interfaces;

namespace Common.Workflows
{
    public class CancelReservationFlow : ReservationFlow
    {
        private IGroupService GroupService { get; }
        public Tour? Tour { get; private set; }
        public Group? Group { get; private set; }

        public CancelReservationFlow(IDepotContext context, ILocalizationService localizationService, 
            ITicketService ticketService, ITourService tourService, IGroupService groupService)
            : base(context, localizationService, ticketService, tourService)
        {
            GroupService = groupService;
        }

        public override (bool Success, string Message) SetTicket(Ticket? ticket)
        {
            var baseResult = base.SetTicket(ticket);
            if (!baseResult.Success)
                return baseResult;

            Tour = TourService.GetTourForTicket(ticket!);

            if (Tour == null)
                return (false, Localization.Get("Flow_no_tour"));

            if (Tour.Departed)
                return (false, Localization.Get("Flow_tour_departed"));

            Group = GroupService.GetGroupForTicket(ticket!);
            if (Group == null)
                return (false, Localization.Get("Flow_no_group"));

            if (Group.GroupOwnerId != ticket!.Id)
                return (false, Localization.Get("Flow_not_group_owner"));

            return baseResult;
        }

        public override (bool Succeeded, string Message) Commit()
        {
            if (Tour == null)
                return (false, Localization.Get("Flow_no_tour"));

            if (Group == null)
                return (false, Localization.Get("Flow_no_group"));

            Group!.GroupTickets.ForEach(groupTicket => Tour!.RegisteredTickets.Remove(groupTicket));
            Context.Groups.Remove(Group);

            return base.Commit();
        }
    }
}
