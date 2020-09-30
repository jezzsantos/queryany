using System.Collections.Generic;
using Application.Resources;

namespace Api.Interfaces.ServiceOperations
{
    public class SearchAvailableCarsResponse : SearchOperationResponse
    {
        public List<Car> Cars { get; set; }
    }
}