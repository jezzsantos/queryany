using System.Collections.Generic;
using Services.Interfaces.Resources;

namespace Services.Interfaces.ServiceOperations
{
    public class SearchAvailableCarsResponse
    {
        public List<Car> Cars { get; set; }
    }
}