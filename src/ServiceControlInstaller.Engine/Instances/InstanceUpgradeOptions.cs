namespace ServiceControlInstaller.Engine.Instances
{
    using System;

    public class InstanceUpgradeOptions
    {
        public bool SuppressRestart { get; set; }
        public bool? OverrideEnableErrorForwarding { get; set; }
        public TimeSpan? ErrorRetentionPeriod { get; set; }
        public TimeSpan? AuditRetentionPeriod { get; set; }
        public string BackupPath { get; set; }
        public bool BackupRavenDbBeforeUpgrade { get; set; }
        public string IngestionCachePath { get; set; }
        public string BodyStoragePath { get; set; }

        public void ApplyChangesToInstance(ServiceControlInstance instance)
        {
            if (OverrideEnableErrorForwarding.HasValue)
            {
                instance.ForwardErrorMessages = OverrideEnableErrorForwarding.Value;
            }

            if (ErrorRetentionPeriod.HasValue)
            {
                instance.ErrorRetentionPeriod = ErrorRetentionPeriod.Value;
            }

            if (AuditRetentionPeriod.HasValue)
            {
                instance.AuditRetentionPeriod = AuditRetentionPeriod.Value;
            }

            instance.ApplyConfigChange();
        }
    }
}