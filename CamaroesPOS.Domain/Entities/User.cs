using System;

namespace CamaroesPOS.Domain.Entities
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
