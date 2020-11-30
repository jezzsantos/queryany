using System.Collections.Generic;
using Application.Resources;

namespace Api.Interfaces.ServiceOperations.Cars
{
    public class SearchAvailableCarsResponse : SearchOperationResponse
    {
        public List<Car> Cars { get; set; }
    }
}