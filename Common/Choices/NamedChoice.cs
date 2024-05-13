namespace Common.Choices
{
    public class NamedChoice<T>
    {
        public string Name { get; set; }
        public T Value { get; set; }

        public NamedChoice(string name, T value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
