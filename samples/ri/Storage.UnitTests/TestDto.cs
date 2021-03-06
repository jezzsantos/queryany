﻿using System;
using Application.Storage.Interfaces;
using QueryAny;

namespace Storage.UnitTests
{
    [EntityName("acontainername")]
    public class TestDto : IPersistableDto
    {
        public string AStringValue { get; set; }

        public string Id { get; set; }

        public DateTime? LastPersistedAtUtc { get; set; }

        public bool? IsDeleted { get; set; }
    }
}