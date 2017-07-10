using Lunch.Domain;
using Lunch.Domain.ErrorHandling;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Lunch.Sheets.Client
{
    public class GoogleSheetsClient: ISheetsClient
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        
        public GoogleSheetsClient()
        {
            HttpClient.BaseAddress = new Uri("https://sheets.googleapis.com/v4/spreadsheets/");
        }

        public void SetAuthorization(string token)
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<SheetsResponse> GetSheetData(string start, string end, string sheetId, string sheetName, SheetsClientConfig config = null)
        {
            var requestUrl = ApplyConfig($"{sheetId}/values/{sheetName}!{start}:{end}", config);
            
            var httpResult = await HttpClient.GetAsync(requestUrl).ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                throw new ApiException("Failed to retrieve data of providers.");
            }
            var result = await MapSheetsResponse(httpResult.Content);
            return result;
        }

        public async Task<List<SheetsInfoResponse>> GetSheetsInfo(string sheetId)
        {
            var httpResult = await HttpClient.GetAsync($"{sheetId}?fields=sheets.properties").ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                throw new ApiException("Failed to retrieve data of providers.");
            }

            var result = await MapSheetsNamesResponse(httpResult.Content).ConfigureAwait(false);
            
            return result;
        }

        public async Task UpdateSheetData(string start, string end, string sheetId, string sheetName, List<List<string>> data)
        {
            var range = $"{sheetName}!{start}:{end}";
            var request = new SheetsRequest
            {
                Range = range,
                MajorDimension = MajorDimension.Rows.ToString(),
                Values = data
            };
            
            var content = new StringContent(JsonConvert.SerializeObject(request, new JsonSerializerSettings{ContractResolver = new CamelCasePropertyNamesContractResolver()}));
            //PUT https://sheets.googleapis.com/v4/spreadsheets/spreadsheetId/values/Sheet1!A1:D5?valueInputOption=USER_ENTERED

            var httpResult = await HttpClient.PutAsync($"{sheetId}/values/{range}?valueInputOption=USER_ENTERED", content).ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                throw new ApiException($"Failed to update data in spreadsheet {sheetId}.");
            }
        }

        public async Task UpdateSheetStyling(string sheetId, SheetStyleRequest request)
        {
            var content = new StringContent(JsonConvert.SerializeObject(request, new JsonSerializerSettings{ContractResolver = new CamelCasePropertyNamesContractResolver()}));

            var httpResult = await HttpClient.PostAsync($"{sheetId}:batchUpdate", content).ConfigureAwait(false);
            if (!httpResult.IsSuccessStatusCode)
            {
                throw new ApiException($"Failed to update styles in spreadsheet {sheetId}.");
            }
        }

        private string ApplyConfig(string url, SheetsClientConfig config)
        {
            var builder = new StringBuilder(url);

            if (config != null)
            {
                if (config.MajorDimension == MajorDimension.Columns)
                {
                    builder.Append("?majorDimension=columns");
                } 
            }

            return builder.ToString();
        }

        private async Task<SheetsResponse> MapSheetsResponse(HttpContent content)
        {
            var jObject = Newtonsoft.Json.Linq.JObject.Parse(await content.ReadAsStringAsync().ConfigureAwait(false));
            SheetsResponse result = new SheetsResponse();
            if (jObject.GetValue("values") == null)
            {
                return new SheetsResponse
                {
                    Rows = new List<List<string>>
                    {
                        new List<string>
                        {
                            "", "", ""
                        }
                    }
                };
            }
            
            foreach (var row in jObject["values"])
            {
                List<string> rowValues = new List<string>();
                foreach (var value in row)
                {
                    rowValues.Add(value.ToString());
                }
                while (rowValues.Count < 2)
                {
                    rowValues.Add("");
                }
                result.Rows.Add(rowValues);
            }

            return result;
        }

        private async Task<List<SheetsInfoResponse>> MapSheetsNamesResponse(HttpContent content)
        {
            var jObject = Newtonsoft.Json.Linq.JObject.Parse(await content.ReadAsStringAsync().ConfigureAwait(false));
            var result = new List<SheetsInfoResponse>();
            foreach (var property in jObject["sheets"])
            {
                result.Add(new SheetsInfoResponse
                {
                    Title = property["properties"]["title"].ToString(),
                    Index = property["properties"]["index"].ToString(),
                    SheetId = property["properties"]["sheetId"].ToString(),
                });
            }

            return result;
        }
    }
}