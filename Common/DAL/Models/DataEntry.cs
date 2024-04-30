namespace Common.DAL.Models
{
    public class DataEntry : DbEntity
    {
        public DateTime TourStart { get; set; }
        public int Entries { get; set; }
    }
}
