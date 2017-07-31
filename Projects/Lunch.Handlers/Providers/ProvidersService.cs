using Lunch.Authorization;
using Lunch.Domain;
using Lunch.Domain.Config;
using Lunch.Sheets.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lunch.Domain.ErrorHandling;
using DayOfWeek = Lunch.Domain.DayOfWeek;

namespace Lunch.Services.Providers
{
    public class ProvidersService: IProvidersService
    {
        private readonly ProviderConfig _providerConfig;
        private readonly GoogleConfig _googleConfig;
        private readonly ISheetsClient _sheetsClient;
        private readonly IAuthorizationService _authService;
        
        public ProvidersService(IOptions<ProviderConfig> providerConfig, IOptions<GoogleConfig> googleConfig, ISheetsClient sheetsClient, IAuthorizationService authService)
        {
            _providerConfig = providerConfig.Value;
            _googleConfig = googleConfig.Value;
            _sheetsClient = sheetsClient;
            _authService = authService;
        }

        public async Task<IList<Provider>> GetProviders(string dayOfWeek)
        {
            _sheetsClient.SetAuthorization(_authService.GetToken());
            var day = ServicesHelper.GetDayOfWeek(dayOfWeek);
            List<Provider> providers;
            
            if (day == DayOfWeek.Friday)
            {
                var sheets = await _sheetsClient.GetSheetsInfo(_googleConfig.FridaySheetId);
                var sheetName = sheets[0].Title;
                var time = DateTime.Parse(sheetName, System.Globalization.CultureInfo.InvariantCulture);
                if(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek).AddDays(5).DayOfYear != time.DayOfYear)
                {
                    throw new ApiException("There's no lunch for this friday yet.", 400);
                }
                var response = await _sheetsClient.GetSheetData(_providerConfig.FromColumnFriday, _providerConfig.ToColumnFriday, _googleConfig.FridaySheetId, sheetName)
                    .ConfigureAwait(false);

                providers = MapFridayProviders(response);
            }
            else
            {
                var sheets = await _sheetsClient.GetSheetsInfo(_googleConfig.SheetId);
                if (sheets.Count < 4)
                {
                    throw new ApiException("Too few sheets on document. Please check that.", 403);
                }
                var sheetName = sheets[(int)day].Title;
                var response = await _sheetsClient.GetSheetData(_providerConfig.FromColumn, _providerConfig.ToColumn, _googleConfig.SheetId, sheetName)
                    .ConfigureAwait(false);

                providers = MapProviders(response);
            }

            return providers;
        }

        private List<Provider> MapProviders(SheetsResponse sheetsResponse)
        {
            List<Provider> providers = new List<Provider>();
            
            var providerList = new Provider();
            List<string> categories = sheetsResponse.Rows.Select(c => c[0]).Where(c => c != "" && c[c.Length - 1] == ':').ToList();
            string currentCategory = "";
            bool firstProvider = true;
            foreach (var row in sheetsResponse.Rows)
            {
                if (categories.Contains(row[0]))
                {
                    currentCategory = row[0];
                    continue;
                }
                if (_providerConfig.Providers.ContainsKey(row[0]))
                {
                    if (!firstProvider)
                    {
                        providers.Add(providerList);
                        providerList = new Provider();
                    }
                    providerList.Name = row[0];
                    firstProvider = false;
                    continue;
                }
                if (!String.IsNullOrEmpty(row[1]))
                {
                    providerList.Dishes.Add(ServicesHelper.MapDish(row[0], row[1], currentCategory.Trim(':'), providerList.Name));
                }
            }
            providers.Add(providerList);

            return providers.Where(p => p.Dishes.Count > 0).ToList();
        }

        private List<Provider> MapFridayProviders(SheetsResponse sheetsResponse)
        {
            List<Provider> providers = new List<Provider>();

            var providerList = new Provider();
            bool firstProvider = true;
            foreach (var row in sheetsResponse.Rows)
            {
                if (_providerConfig.FridayProviders.ContainsKey(row[0]))
                {
                    if (!firstProvider)
                    {
                        providers.Add(providerList);
                        providerList = new Provider();
                    }
                    providerList.Description = sheetsResponse.Rows[0][0].Replace("Supplier-", "");
                    providerList.Name = UpperCaseToPascalCase(row[0]);
                    firstProvider = false;
                    continue;
                }
                if (!String.IsNullOrEmpty(row[1]))
                {
                    var isSoup = _providerConfig.FridayProviders.Select(p => p.Key.ToLowerInvariant()).ToList().IndexOf(providerList.Name.ToLowerInvariant()) == 1;
                    providerList.Dishes.Add(ServicesHelper.MapFridayDish(row[0], row[1], isSoup, providerList.Name));
                }
            }
            
            providers.Add(providerList);
            return providers;
        }

        private string UpperCaseToPascalCase(string uppercase)
        {
            var words = uppercase.Split(' ');
            string result = "";
            foreach (var word in words)
            {
                result += word[0] + word.Substring(1).ToLowerInvariant() + " ";
            }

            return result.Trim();
        }
    }
}