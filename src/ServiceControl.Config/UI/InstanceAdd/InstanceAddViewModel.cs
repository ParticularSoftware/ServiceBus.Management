﻿namespace ServiceControl.Config.UI.InstanceAdd
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Windows.Input;
    using ServiceControl.Config.Commands;
    using ServiceControlInstaller.Engine.Instances;
    using SharedInstanceEditor;
    using Validar;

    [InjectValidation]
    public class InstanceAddViewModel : SharedInstanceEditorViewModel
    {
        public InstanceAddViewModel()
        {
            DisplayName = "Add new instance";

            SelectDestinationPath = new SelectPathCommand(p => DestinationPath = p, isFolderPicker: true, defaultPath: DestinationPath);
            SelectDatabasePath = new SelectPathCommand(p => DatabasePath = p, isFolderPicker: true, defaultPath: DatabasePath);
            SelectLogPath = new SelectPathCommand(p => LogPath = p, isFolderPicker: true, defaultPath: LogPath);

            var serviceControlInstances = ServiceControlInstance.Instances();

            // Defaults
            if (!serviceControlInstances.Any())
            {
                InstanceName = "Particular.ServiceControl";
                PortNumber = "33333";
            }
            else
            {
                var i = 0;
                while (true)
                {
                    InstanceName = string.Format("Particular.ServiceControl.{0}", ++i);
                    if (!serviceControlInstances.Any(p => p.Name.Equals(InstanceName, StringComparison.OrdinalIgnoreCase)))
                    {
                        break;
                    }
                }
            }
            Description = "A ServiceControl Instance";
            HostName = "localhost"; 
            AuditQueueName = "audit";
            AuditForwardingQueueName = "audit.log";
            ErrorQueueName = "error";
            ErrorForwardingQueueName = "error.log";
        }

        public string DestinationPath { get; set; }
        public ICommand SelectDestinationPath { get; private set; }

        public string DatabasePath { get; set; }
        public ICommand SelectDatabasePath { get; private set; }

        protected override void OnInstanceNameChanged()
        {
            DestinationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Particular Software", InstanceName);
            DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Particular", "ServiceControl", InstanceName, "DB");
            LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Particular", "ServiceControl", InstanceName, "Logs");
        }
    }
}