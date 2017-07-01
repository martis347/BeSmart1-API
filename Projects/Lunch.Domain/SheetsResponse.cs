using System.Collections.Generic;

namespace Lunch.Domain
{
    public class SheetsResponse
    {
        public SheetsResponse()
        {
            Rows = new List<List<string>>();
        }

        public List<List<string>> Rows { get; set; }
    }
}