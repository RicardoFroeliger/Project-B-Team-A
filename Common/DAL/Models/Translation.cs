namespace Common.DAL.Models
{
    public class Translation : DbEntity
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public string Locale { get; set; } = "nl-NL";
    }
}
