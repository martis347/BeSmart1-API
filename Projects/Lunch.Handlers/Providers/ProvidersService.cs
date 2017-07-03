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
            var day = GetDayOfWeek(dayOfWeek);
            List<Provider> providers;
            
            if (day == DayOfWeek.Friday)
            {
                var sheets = await _sheetsClient.GetSheetNames(_googleConfig.FridaySheetId);
                var sheetName = sheets[0];
                var response = await _sheetsClient.GetSheetData(_providerConfig.FromColumnFriday, _providerConfig.ToColumnFriday, _googleConfig.FridaySheetId, sheetName)
                    .ConfigureAwait(false);

                providers = MapFridayProviders(response);
            }
            else
            {
                var sheets = await _sheetsClient.GetSheetNames(_googleConfig.SheetId);
                if (sheets.Count < 4)
                {
                    throw new ApiException("Too few sheets on document. Please check that.", 403);
                }
                var sheetName = sheets[day == DayOfWeek.Friday ? 0 : (int)day];
                var response = await _sheetsClient.GetSheetData(_providerConfig.FromColumn, _providerConfig.ToColumn, _googleConfig.SheetId, sheetName)
                    .ConfigureAwait(false);

                providers = MapProviders(response);
            }

            return providers;
        }

        private DayOfWeek GetDayOfWeek(string dayOfWeek)
        {
            DayOfWeek result;
            if (!Enum.TryParse(dayOfWeek, true, out result))
            {
                throw new ApiException("Incorrect day of week provided", 400);
            }

            return result;
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
                if (_providerConfig.ProvidersNames.Contains(row[0]))
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
                    providerList.Dishes.Add(MapDish(row[0], row[1], currentCategory.Trim(':')));
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
                if (_providerConfig.FridayProvidersNames.Contains(row[0]))
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
                    providerList.Dishes.Add(MapFridayDish(row[0], row[1], providerList.Name == _providerConfig.FridayProvidersNames[0]));
                }
            }
            
            providers.Add(providerList);
            return providers;
        }

        private Dish MapDish(string value, string price, string category)
        {
            double priceValue = Double.Parse(price.Trim('€').Replace(',', '.'));

            Dish result;
            string[] splitValue = value.Split('+');
            if (splitValue.Length > 1)
            {
                string mainDish = splitValue[0];
                List<string> sideDishes = new List<string>();
                if (splitValue.Length > 1)
                {
                    string[] splitSideDishValue = splitValue[1].Split(new[] { "ARBA" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var dish in splitSideDishValue)
                    {
                        sideDishes.Add(dish.Trim());
                    }
                }

                result = new Dish
                {
                    DishType = DishType.Combined,
                    Name = value.Trim(),
                    Price = priceValue,
                    Category = category,
                    MainDishes = new List<string> { mainDish.Trim() },
                    SideDishes = sideDishes
                };
            }
            else if (priceValue < 2)
            {
                result = new Dish
                {
                    DishType = DishType.Side,
                    Name = value.Trim(),
                    Price = priceValue,
                    Category = category
                };
            }
            else
            {
                result = new Dish
                {
                    DishType = DishType.Main,
                    Name = value.Trim(),
                    Price = priceValue,
                    Category = category
                };
            }
            
            return result;
        }

        private Dish MapFridayDish(string name, string count, bool isSoup)
        {
            var result = new Dish
            {
                DishType = isSoup ? DishType.Side : DishType.Main,
                Name = name.Trim(),
                Count = Int32.Parse(count.Trim())
            };

            return result;
        }
    }
}