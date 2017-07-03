using System.Collections.Generic;
using System.Threading.Tasks;
using Lunch.Domain;

namespace Lunch.Sheets.Client
{
    public interface ISheetsClient
    {
        void SetAuthorization(string token);
        Task<SheetsResponse> GetSheetData(string x, string y, string sheetId, string sheetName, SheetsClientConfig config = null);
        Task<List<string>> GetSheetNames(string sheetName);
    }
}