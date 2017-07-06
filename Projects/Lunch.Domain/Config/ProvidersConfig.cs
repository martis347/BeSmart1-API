using System.Collections.Generic;

namespace Lunch.Domain.Config
{
    public class ProviderConfig
    {
        public string FromColumn { get; set; }

        public string ToColumn { get; set; }

        public string FromColumnFriday { get; set; }

        public string ToColumnFriday { get; set; }
        
        public Dictionary<string, string> Providers { get; set; }
        public Dictionary<string, string> FridayProviders { get; set; }
    }
}