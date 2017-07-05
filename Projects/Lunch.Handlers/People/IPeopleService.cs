using System.Collections.Generic;
using System.Threading.Tasks;
using Lunch.Domain;

namespace Lunch.Services.People
{
    public interface IPeopleService
    {
        Task<IList<Person>> GetPeople();
    }
}