﻿using System;

namespace DotVVM.Utils.ConfigurationHost.Output.Statistics
{
    public class OperationStatistics
    {
        public string Name { get; set; }
        public int Skipped { get; set; }
        public int Executed { get; set; }
        public int Successful { get; set; }
    }
}