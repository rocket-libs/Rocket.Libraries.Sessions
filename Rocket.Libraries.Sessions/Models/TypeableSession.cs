using System;
using System.Collections.Generic;
using System.Text;

namespace Rocket.Libraries.Sessions.Models
{
    public class TypeableSession<TIdentifier>
    {
        public object Key { get; set; }

        public string TimezoneId { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DisplayName => $"{FirstName} {LastName}".Trim();

        public TIdentifier UserId { get; set; }

        public TIdentifier CompanyId { get; set; }
    }
}