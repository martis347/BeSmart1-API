using System;
using Lunch.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lunch.Domain;
using Lunch.Domain.Config;
using Lunch.Domain.ErrorHandling;
using Lunch.Sheets.Client;
using Microsoft.Extensions.Options;
using DayOfWeek = Lunch.Domain.DayOfWeek;

namespace Lunch.Services.Dishes
{
    public class DishesService: IDishesService
    {
        private readonly ProviderConfig _providerConfig;
        private readonly PeopleConfig _peopleConfig;
        private readonly GoogleConfig _googleConfig;
        private readonly DishesConfig _dishesConfig;
        private readonly ISheetsClient _sheetsClient;
        private readonly IAuthorizationService _authService;

        public DishesService(IOptions<ProviderConfig> providerConfig, IOptions<PeopleConfig> peopleConfig, IOptions<DishesConfig> dishesConfig, IOptions<GoogleConfig> googleConfig, ISheetsClient sheetsClient, IAuthorizationService authService)
        {
            _peopleConfig = peopleConfig.Value;
            _googleConfig = googleConfig.Value;
            _providerConfig = providerConfig.Value;
            _dishesConfig = dishesConfig.Value;
            _sheetsClient = sheetsClient;
            _authService = authService;
        }
        
        public async Task SelectDishes(UserSelection selection, string dayOfWeek)
        {
            _sheetsClient.SetAuthorization(_authService.GetToken());

            var day = ServicesHelper.GetDayOfWeek(dayOfWeek);

            string sheetId = _googleConfig.SheetId,
                fromColumn = _peopleConfig.FromColumn,
                toColumn = _peopleConfig.ToColumn,
                destinationFromColumn = _dishesConfig.FromColumnLetter,
                destinationToColumn = _dishesConfig.ToColumnLetter;

            List<List<string>> request = new List<List<string>> {new List<string> { selection.SideDish != null ? selection.SideDish.Name : "", selection.MainDish != null ? selection.MainDish.Name : "" } };
            var providers = _providerConfig.Providers;
            bool addCurrencyFormatting = true;
            
            if (day == DayOfWeek.Friday)
            {
                sheetId = _googleConfig.FridaySheetId;
                fromColumn = _peopleConfig.FromColumnFriday;
                toColumn = _peopleConfig.ToColumnFriday;
                destinationFromColumn = _dishesConfig.FromColumnFridayLetter;
                destinationToColumn = _dishesConfig.ToColumnFridayLetter;
                providers = _providerConfig.FridayProviders;
                addCurrencyFormatting = false;
            }
            else
            {
                if (!String.IsNullOrEmpty(selection.Price) && selection.Price != "0")
                {
                    request[0].Add(selection.Price.Replace('.', ','));
                }
            }

            var sheets = await _sheetsClient.GetSheetsInfo(sheetId).ConfigureAwait(false);
            var response = await _sheetsClient.GetSheetData(fromColumn, toColumn, sheetId, sheets[0].Title)
                .ConfigureAwait(false);

            var rowNumber = response.Rows.Select(r => r[0].ToLowerInvariant()).ToList().IndexOf(selection.UserName.ToLowerInvariant());
            if (rowNumber++ == -1)
            {
                throw new ApiException("User with given name not found in sheet", 400);
            }
            
            destinationFromColumn = destinationFromColumn + rowNumber;
            destinationToColumn = destinationToColumn + rowNumber;

            var stylesheetRequest = BuildSheetStyleRequest(selection, providers, sheets[0].SheetId,
                rowNumber - 1, char.ToUpper(fromColumn[0]) - 64, addCurrencyFormatting);

            await _sheetsClient.UpdateSheetStyling(sheetId, stylesheetRequest);

            await _sheetsClient.UpdateSheetData(destinationFromColumn, destinationToColumn, sheetId, sheets[0].Title, request).ConfigureAwait(false);
        }

        public async Task<SelectedDishesResponse> GetSelectedDishes(string dayOfWeek, string username)
        {
            _sheetsClient.SetAuthorization(_authService.GetToken());

            var day = ServicesHelper.GetDayOfWeek(dayOfWeek);

            string sheetId = _googleConfig.SheetId,
                fromColumn = _peopleConfig.FromColumn,
                toColumn = _peopleConfig.ToColumn,
                destinationFromColumn = _dishesConfig.FromColumnLetter,
                destinationToColumn = _dishesConfig.ToColumnLetter;

            if (day == DayOfWeek.Friday)
            {
                sheetId = _googleConfig.FridaySheetId;
                fromColumn = _peopleConfig.FromColumnFriday;
                toColumn = _peopleConfig.ToColumnFriday;
                destinationFromColumn = _dishesConfig.FromColumnFridayLetter;
                destinationToColumn = _dishesConfig.ToColumnFridayLetter;
            }

            var sheets = await _sheetsClient.GetSheetsInfo(sheetId).ConfigureAwait(false);
            var response = await _sheetsClient.GetSheetData(fromColumn, toColumn, sheetId, sheets[0].Title)
                .ConfigureAwait(false);

            var rowNumber = response.Rows.Select(r => r[0].ToLowerInvariant()).ToList().IndexOf(username.ToLowerInvariant());
            if (rowNumber++ == -1)
            {
                throw new ApiException("User with given name not found in sheet", 400);
            }

            destinationFromColumn = destinationFromColumn + rowNumber;
            destinationToColumn = destinationToColumn + rowNumber;

            var clientResult = await _sheetsClient.GetSheetData(destinationFromColumn, destinationToColumn, sheetId, sheets[0].Title);

            var result = new SelectedDishesResponse
            {
                MainDish = clientResult.Rows[0][1].Length > 4 ? clientResult.Rows[0][1] : null,
                SideDish = clientResult.Rows[0][0].Length > 4 ? clientResult.Rows[0][0] : null,
                Price = clientResult.Rows[0].Count > 2 && clientResult.Rows[0][2].Contains('€') ? clientResult.Rows[0][2] : null
            };

            return result;
        }

        private SheetStyleRequest BuildSheetStyleRequest(UserSelection selection, Dictionary<string, string> providers, string sheetId, int sideDishX, int sideDishY, bool addCurrencyFormatting)
        {
            var font = selection.SideDish != null && String.IsNullOrEmpty(selection.SideDish.Provider) ? "Arial" : "Times New Roman";


            var request = new SheetStyleRequest
            {
                Requests = new List<StyleSheetPartRequest>
                {
                    new StyleSheetPartRequest
                    {
                        RepeatCell =  new RepeatCell
                        {
                            Fields = "*",
                            Range = new Range
                            {
                                SheetId = sheetId,
                                StartColumnIndex = sideDishY,
                                EndColumnIndex = sideDishY + 1,
                                StartRowIndex = sideDishX,
                                EndRowIndex = sideDishX + 1
                            },
                            Cell = new Cell
                            {
                                UserEnteredFormat = new UserEnteredFormat
                                {
                                    TextFormat = new TextFormat
                                    {
                                        FontFamily = font,
                                        FontSize = "12",
                                        ForegroundColor = new ForegroundColor
                                        {
                                            Green = selection.SideDish != null && providers.ContainsKey(selection.SideDish.Provider) && providers[selection.SideDish.Provider] == "green" ? "120" : "0",
                                            Red = selection.SideDish != null && providers.ContainsKey(selection.SideDish.Provider) && providers[selection.SideDish.Provider] == "red" ? "120" : "0"
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new StyleSheetPartRequest
                    {
                        RepeatCell =  new RepeatCell
                        {
                            Fields = "*",
                            Range = new Range
                            {
                                SheetId = sheetId,
                                StartColumnIndex = sideDishY + 1,
                                EndColumnIndex = sideDishY + 2,
                                StartRowIndex = sideDishX,
                                EndRowIndex = sideDishX + 1
                            },
                            Cell = new Cell
                            {
                                UserEnteredFormat = new UserEnteredFormat
                                {
                                    TextFormat = new TextFormat
                                    {
                                        FontFamily = font,
                                        FontSize = "12",
                                        ForegroundColor = new ForegroundColor
                                        {
                                            Green = selection.MainDish != null &&providers.ContainsKey(selection.MainDish.Provider) && providers[selection.MainDish.Provider] == "green" ? "120" : "0",
                                            Red = selection.MainDish != null &&providers.ContainsKey(selection.MainDish.Provider) && providers[selection.MainDish.Provider] == "red" ? "120" : "0"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            if (addCurrencyFormatting)
            {
                request.Requests.Add(new StyleSheetPartRequest
                {
                    RepeatCell = new RepeatCell
                    {
                        Fields = "*",
                        Range = new Range
                        {
                            SheetId = sheetId,
                            StartColumnIndex = sideDishY + 2,
                            EndColumnIndex = sideDishY + 3,
                            StartRowIndex = sideDishX,
                            EndRowIndex = sideDishX + 1
                        },
                        Cell = new Cell
                        {
                            UserEnteredFormat = new UserEnteredFormat
                            {
                                TextFormat = new TextFormat
                                {
                                    FontFamily = font,
                                    FontSize = "12"
                                },
                                NumberFormat = new NumberFormat
                                {
                                    Type = "CURRENCY",
                                    Pattern = "€0.00"
                                }
                            }
                        }
                    }
                });
            }

            return request;
        }
    }
}
