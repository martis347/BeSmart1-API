using System.Collections.Generic;

namespace Lunch.Domain
{
    public class SheetsRequest
    {
        public string Range { get; set; }
        public string MajorDimension { get; set; }
        public List<List<string>> Values { get; set; }
        
    }
}