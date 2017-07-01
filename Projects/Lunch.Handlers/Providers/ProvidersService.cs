using Lunch.Authorization;
using Lunch.Domain;
using Lunch.Domain.Config;
using Lunch.Sheets.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lunch.Services.Providers
{
    public class ProvidersService: IProvidersService
    {
        private readonly ProviderConfig _providerConfig;
        private readonly ISheetsClient _sheetsClient;
        private readonly IAuthorizationService _authService;
        
        public ProvidersService(IOptions<ProviderConfig> providerConfig, ISheetsClient sheetsClient, IAuthorizationService authService)
        {
            _providerConfig = providerConfig.Value;
            _sheetsClient = sheetsClient;
            _authService = authService;
        }

        public async Task<IList<Provider>> GetProviders(DateTime time)
        {
            _sheetsClient.SetAuthorization(_authService.GetToken());

            var response = await _sheetsClient.GetSheetData(_providerConfig.FromColumn, _providerConfig.ToColumn, "Lapas1")
                .ConfigureAwait(false);

            var providers = MapProviders(response);

            return providers;
        }

        private IList<Provider> MapProviders(SheetsResponse sheetsResponse)
        {
            IList<Provider> providers = new List<Provider>();
            
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
    }
}