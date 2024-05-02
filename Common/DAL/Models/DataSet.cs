namespace Common.DAL.Models
{
    public class DataSet : DbEntity
    {
        public int CreatorId { get; set; }
        public string CreatorName { get; set; } = "";
        public DateTime CreationDate { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<DataEntry> Entries { get; set; } = new List<DataEntry>();
    }
}
