using Common.DAL.Interfaces;
using Common.DAL.Models;
using Common.Services.Interfaces;

namespace Common.Workflows
{
    public class RemoveTicketTourGuideFlow : TourGuideFlow
    {
        private IGroupService GroupService { get; }

        public RemoveTicketTourGuideFlow(IDepotContext context, ILocalizationService localizationService, ITicketService ticketService, ITourService tourService, IGroupService groupService)
            : base(context, localizationService, ticketService, tourService)
        {
            GroupService = groupService;
        }

        public (bool Success, string Message) RemoveTicket(int ticketNumber, bool deleteGroup)
        {
            var validation = ValidateTicket(ticketNumber);
            if (!validation.Success)
                return validation;

            if (!Tour!.RegisteredTickets.Contains(ticketNumber))
                return (false, Localization.Get("Flow_ticket_not_in_tour"));

            if (TicketBuffer.Keys.ToList().Contains(ticketNumber))
                return (false, Localization.Get("Flow_ticket_already_added_to_list"));

            var group = GroupService.GetGroupForTicket(ticketNumber)!;
            if (group.GroupTickets.Count > 1 && deleteGroup)
                foreach (int ticket in group.GroupTickets)
                    TicketBuffer.Add(ticket, deleteGroup);
            else
                TicketBuffer.Add(ticketNumber, deleteGroup);

            return (true, Localization.Get("Flow_ticket_added_to_list"));
        }


        public override (bool Success, string Message) SetTour(Tour? tour)
        {
            var baseResult = base.SetTour(tour);
            if (!baseResult.Success)
                return baseResult;

            if (!tour!.RegisteredTickets.Any())
                return (false, Localization.Get("Flow_tour_no_tickets_in_tour"));

            return (true, Localization.Get("flow_tour_is_valid"));
        }

        public override (bool Succeeded, string Message) Commit()
        {
            if (!TicketBuffer.Any())
                return (false, Localization.Get("Flow_no_tickets_to_remove"));

            foreach ((int ticketNumber, bool deleteGroup) in TicketBuffer)
            {
                var group = GroupService.GetGroupForTicket(ticketNumber)!;

                if (!Tour!.RegisteredTickets.Contains(ticketNumber))
                    continue; // Was added as a way to display the impact of deleting a group

                if (group.GroupOwnerId == ticketNumber) // I am the group owner
                {
                    if (deleteGroup) // Delete my group
                    {
                        group.GroupTickets.ForEach(ticket => Tour!.RegisteredTickets.Remove(ticket));
                        GroupService.RemoveOne(group);
                    }
                    else // Delete my group, but keep them in the tour as individuals
                    {
                        Tour!.RegisteredTickets.Remove(ticketNumber);

                        group.GroupTickets.Remove(ticketNumber);
                        foreach (int ticket in group.GroupTickets)
                        {
                            GroupService.AddOne(new Group() { GroupOwnerId = ticket, GroupTickets = new() { ticket } });
                        }
                        GroupService.RemoveOne(group);
                    }
                }
                else // I am in a group, remove me from the group and tour
                {
                    group.GroupTickets.Remove(ticketNumber);
                    Tour!.RegisteredTickets.Remove(ticketNumber);
                }
            }

            return base.Commit();
        }

        public override (bool Succeeded, string Message) Rollback()
        {
            TicketBuffer.Clear();

            return base.Rollback();
        }
    }
}
