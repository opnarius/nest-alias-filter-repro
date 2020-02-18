using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace repo
{
    public class User
    {
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string State { get; set; }

        public bool IsActive { get; set; }

        public int Number { get; set; }

        public DateTime RegistrationDate { get; set; }

        public string RegistrationTime => RegistrationDate.ToLongTimeString();

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string EmptyValue { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public DateTime? LastOrderDate { get; set; }
    }
}
