namespace Common.Choices
{
    public class TimeChoice
    {
        public TimeSpan Span { get; set; }

        public TimeChoice(TimeSpan span)
        {
            Span = span;
        }

        public override string ToString()
        {
            return Span.ToString("hh\\:mm");
        }
    }
}
