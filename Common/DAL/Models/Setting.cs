namespace Common.DAL.Models
{
    public class Setting : DbEntity
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }
}
