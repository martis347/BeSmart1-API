using System.Collections.Generic;

namespace Lunch.Domain.Config
{
    public class ProviderConfig
    {
        public string FromColumn { get; set; }

        public string ToColumn { get; set; }

        public string FromColumnFriday { get; set; }

        public string ToColumnFriday { get; set; }
        
        public List<string> ProvidersNames { get; set; }
        public List<string> FridayProvidersNames { get; set; }
    }
}