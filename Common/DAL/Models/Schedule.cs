namespace Common.DAL.Models
{
    public class Schedule : DbEntity
    {
        public DayOfWeek Weekday { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
