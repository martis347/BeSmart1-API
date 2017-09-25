using System;
using Lunch.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lunch.Domain;
using Lunch.Domain.Config;
using Lunch.Sheets.Client;
using Microsoft.Extensions.Options;

namespace Lunch.Services.People
{
    public class PeopleService: IPeopleService
    {
        private readonly PeopleConfig _peopleConfig;
        private readonly GoogleConfig _googleConfig;
        private readonly ISheetsClient _sheetsClient;
        private readonly IAuthorizationService _authService;
        
        public PeopleService(IOptions<PeopleConfig> peopleConfig, IOptions<GoogleConfig> googleConfig, ISheetsClient sheetsClient, IAuthorizationService authService)
        {
            _peopleConfig = peopleConfig.Value;
            _googleConfig = googleConfig.Value;
            _sheetsClient = sheetsClient;
            _authService = authService;
        }
        
        public async Task<IList<Person>> GetPeople()
        {
            _sheetsClient.SetAuthorization(_authService.GetToken());

            var sheets = await _sheetsClient.GetSheetsInfo(_googleConfig.FridaySheetId).ConfigureAwait(false);
            var sheetsRespone = await _sheetsClient
                .GetSheetData(_peopleConfig.FromColumnFriday, _peopleConfig.ToColumnFriday, _googleConfig.FridaySheetId, sheets[0].Title)
                .ConfigureAwait(false);

            var result = sheetsRespone.Rows
                .Where(r => !String.IsNullOrEmpty(r.FirstOrDefault()))
                .Select(r => new Person(r.FirstOrDefault()))
                .ToList();
            
            return result;
        }
    }
}