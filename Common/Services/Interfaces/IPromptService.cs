using Common.Choices;
using Common.DAL.Models;
using Common.Enums;

namespace Common.Services.Interfaces
{
    public interface IPromptService
    {
        int AskNumber(string questionKey, string validationErrorKey, int? min = null, int? max = null);
        int AskTicketNumber();
        int AskTicketNumberOrUserpass();
        int AskUserpass();
        List<DayOfWeek> AskSchedule();
        User AskUser(List<User> options);
        DateTime AskDate(string titleTranslationKey, string moreOptionsTranslationKey, int dateRange = 31, DateTime? startDate = null);
        TimeSpan AskTime(string titleTranslationKey, string moreOptionsTranslationKey, int timeInterval = 30, int startTime = 0);
        NavigationChoice GetMenu(string titleTranslationKey, string moreOptionsTranslationKey, List<NavigationChoice> navigationChoices, User? user = null);
        Tour AskTour(string titleTranslationKey, string moreOptionsTranslationKey, int minimumCapacity, int recentTours = 0, int upcomingTours = -1);
        bool AskConfirmation(string titleTranslationKey);
        string AskUsername();
        Role AskRole();
    }
}
