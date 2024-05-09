using Common.Choices;
using Common.DAL.Models;
using Common.Enums;

namespace Common.Services
{
    public interface IPromptService
    {
        int AskNumber(string questionKey, string validationErrorKey, int? min = null, int? max = null);
        int AskTicketNumber();
        int AskTicketNumberOrUserpass();
        int AskUserId();
        List<DayOfWeek> AskSchedule();
        User AskUser(List<User> options);
        DateTime AskDate(string titleTranslationKey, string moreOptionsTranslationKey, int dateRange = 31, DateTime? startDate = null, bool historical = false);
        TimeSpan AskTime(string titleTranslationKey, string moreOptionsTranslationKey, int timeInterval = 30, int startTime = 0);
        Action GetMenu(string titleTranslationKey, string moreOptionsTranslationKey, List<NamedChoice<Action>> navigationChoices, User? user = null);
        Tour AskTour(string titleTranslationKey, string moreOptionsTranslationKey, int minimumCapacity, int recentTours = 0, int upcomingTours = -1);
        bool AskConfirmation(string titleTranslationKey);
        string AskUsername();
        RoleType AskRole();
        string AskFilePath(string titleTranslationKey);
    }
}
