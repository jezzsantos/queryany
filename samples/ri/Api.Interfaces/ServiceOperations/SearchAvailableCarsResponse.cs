using System.Collections.Generic;
using Domain.Interfaces.Resources;

namespace Api.Interfaces.ServiceOperations
{
    public class SearchAvailableCarsResponse : SearchOperationResponse
    {
        public List<Car> Cars { get; set; }
    }
}