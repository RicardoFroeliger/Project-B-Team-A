namespace Common.Choices
{
    public class BoolChoice
    {
        public string Name { get; set; }
        public bool Choice { get; set; }

        public BoolChoice(string name, bool choice)
        {
            Name = name;
            Choice = choice;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
