using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lunch.Domain;

namespace Lunch.Services.Providers
{
    public interface IProvidersService
    {
        Task<IList<Provider>> GetProviders(string dayOfWeek);
    }
}