namespace Common.DAL.Models
{
    public class Ticket : DbEntity
    {
        public DateTime ValidOn { get; set; }
        public bool Expires { get; set; } = false;
    }
}
