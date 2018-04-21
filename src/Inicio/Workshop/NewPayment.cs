using System;

namespace Workshop
{
    public class NewPayment
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Email { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public bool Valid { get; set; }
        public string Transaction { get; set; }
    }
}
