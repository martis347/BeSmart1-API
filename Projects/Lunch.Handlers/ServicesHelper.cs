using System;
using System.Collections.Generic;
using Lunch.Domain;
using Lunch.Domain.ErrorHandling;
using DayOfWeek = Lunch.Domain.DayOfWeek;

namespace Lunch.Services
{
    public static class ServicesHelper
    {
        public static DayOfWeek GetDayOfWeek(string day)
        {
            DayOfWeek result;
            if (!Enum.TryParse(day, true, out result))
            {
                throw new ApiException("Incorrect day of week provided", 400);
            }

            return result;
        }
        
        public static Dish MapFridayDish(string name, string count, bool isSoup, string providerName)
        {
            var result = new Dish
            {
                DishType = isSoup ? DishType.Side : DishType.Main,
                Name = name.Trim(),
                ProviderName = providerName,
                Count = Int32.Parse(count.Trim())
            };

            return result;
        }
        
        public static Dish MapDish(string value, string price, string category, string providerName)
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
                    ProviderName = providerName,
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
                    ProviderName = providerName,
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
                    ProviderName = providerName,
                    Price = priceValue,
                    Category = category
                };
            }

            return result;
        }
    }
}