using System;

namespace Rocket.Libraries.Sessions.Models
{
    public class Session
    {
        public Guid Key { get; set; }

        public string TimezoneId { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DisplayName => $"{FirstName} {LastName}".Trim();

        public object UserId { get; set; }

        public object CompanyId { get; set; }
    }
}