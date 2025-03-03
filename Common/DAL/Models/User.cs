﻿namespace Common.DAL.Models
{
    public class User : DbEntity
    {
        public int Role { get; set; }
        public string Name { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public List<Schedule> Planning { get; set; } = new List<Schedule>();
    }
}
