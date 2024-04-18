namespace Common.DAL.Models
{
    public class Group : DbEntity
    {
        public int GroupOwnerId { get; set; }
        public List<int> GroupTickets { get; set; } = new List<int>();
    }
}
