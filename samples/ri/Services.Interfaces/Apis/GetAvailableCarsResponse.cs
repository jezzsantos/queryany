using System.Collections.Generic;
using Services.Interfaces.Resources;

namespace Services.Interfaces.Apis
{
    public class GetAvailableCarsResponse
    {
        public List<Car> Cars { get; set; }
    }
}