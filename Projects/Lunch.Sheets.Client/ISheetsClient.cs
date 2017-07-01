using System.Threading.Tasks;
using Lunch.Domain;

namespace Lunch.Sheets.Client
{
    public interface ISheetsClient
    {
        void SetAuthorization(string token);
        Task<SheetsResponse> GetSheetData(string x, string y, string sheetName, SheetsClientConfig config = null);
    }
}