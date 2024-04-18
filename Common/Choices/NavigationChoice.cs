namespace Common.Choices
{
    public class NavigationChoice
    {
        public string Name { get; set; }
        public Action NavigationAction { get; set; }

        public NavigationChoice(string name, Action navigationAction)
        {
            Name = name;
            NavigationAction = navigationAction;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
