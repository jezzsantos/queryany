using System.Collections.Generic;
using Application.Common.Resources;

namespace Api.Interfaces.ServiceOperations.Cars
{
    public class SearchAvailableCarsResponse : SearchOperationResponse
    {
        public List<Car> Cars { get; set; }
    }
}