
using Common.DAL.Models;

namespace Common.Choices
{
    public class UserChoice
    {
        public string Name { get; set; }
        public User User { get; set; }

        public UserChoice(string name, User user)
        {
            Name = name;
            User = user;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
