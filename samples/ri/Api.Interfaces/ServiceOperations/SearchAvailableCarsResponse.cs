using System.Collections.Generic;
using Services.Interfaces.Resources;

namespace Api.Interfaces.ServiceOperations
{
    public class SearchAvailableCarsResponse : SearchOperationResponse
    {
        public List<Car> Cars { get; set; }
    }
}