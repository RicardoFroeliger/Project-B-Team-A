namespace Common.Choices
{
    public class DateChoice
    {
        public DateTime Date { get; set; }

        public DateChoice(DateTime date)
        {
            Date = date;
        }

        public override string ToString()
        {
            return Date.ToShortDateString();
        }
    }
}
