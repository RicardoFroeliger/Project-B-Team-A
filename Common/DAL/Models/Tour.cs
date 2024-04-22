namespace Common.DAL.Models
{
    public class Tour : DbEntity
    {
        public DateTime Start { get; set; }
        public List<int> RegisteredTickets { get; set; } = new List<int>();
        public bool Departed { get; set; } = false;
        public int GuideId { get; set; } = 0;
        public int PlannedGuideId { get; set; } = 0;
    }
}
