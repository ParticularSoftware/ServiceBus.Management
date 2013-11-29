﻿namespace ServiceControl.Plugin.CustomChecks.Messages
{
    using System;

    public class ReportCustomCheckResult
    {
        public string CustomCheckId { get; set; }
        public string Category { get; set; }
        public CheckResult Result { get; set; }
        public DateTime ReportedAt { get; set; }
    }
}
