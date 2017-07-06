using System;
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
    }
}