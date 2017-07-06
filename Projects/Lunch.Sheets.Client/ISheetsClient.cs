using System.Collections.Generic;
using System.Threading.Tasks;
using Lunch.Domain;

namespace Lunch.Sheets.Client
{
    public interface ISheetsClient
    {
        void SetAuthorization(string token);
        Task<SheetsResponse> GetSheetData(string start, string end, string sheetId, string sheetName, SheetsClientConfig config = null);
        Task<List<SheetsInfoResponse>> GetSheetsInfo(string sheetName);
        Task UpdateSheetData(string start, string end, string sheetId, string sheetName, List<List<string>> data);
        Task UpdateSheetStyling(string sheetId, SheetStyleRequest request);
    }
}