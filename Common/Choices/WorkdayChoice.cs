namespace Common.Choices
{
    public class WorkdayChoice
    {
        public string Name { get; set; }
        public DayOfWeek Day { get; set; }

        public WorkdayChoice(string name, DayOfWeek day)
        {
            Name = name;
            Day = day;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
