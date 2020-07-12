﻿using System;
using ServiceStack;

namespace Services.Interfaces.Apis
{
    [Route("/cars/{Id}/occupy", "PUT")]
    public class OccupyCarRequest : IReturn<OccupyCarResponse>, IPut
    {
        public string Id { get; set; }

        public DateTime UntilUtc { get; set; }
    }
}