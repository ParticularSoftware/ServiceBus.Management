namespace ServiceControl.Config.UI.InstanceAdd
{
    using FluentValidation;
    using Validation;

    public class ServiceControlAddViewModelValidator : SharedServiceControlEditorViewModelValidator<ServiceControlAddViewModel>
    {
        public ServiceControlAddViewModelValidator()
        {
            RuleFor(x => x.ServiceAccount)
                .NotEmpty()
                .When(x => x.SubmitAttempted);

            RuleFor(x => x.SelectedTransport)
                .NotEmpty();

            RuleFor(x => x.PortNumber)
                .NotEmpty()
                .ValidPort()
                .PortAvailable()
                .MustNotBeIn(x => UsedPorts(x.InstanceName))
                .NotEqual(x => x.MaintenancePortNumber)
                .WithMessage(Validations.MSG_MUST_BE_UNIQUE, "Ports")
                .When(x => x.SubmitAttempted);

            RuleFor(x => x.MaintenancePortNumber)
                .NotEmpty()
                .ValidPort()
                .PortAvailable()
                .MustNotBeIn(x => UsedPorts(x.InstanceName))
                .NotEqual(x => x.PortNumber)
                .WithMessage(Validations.MSG_MUST_BE_UNIQUE, "Ports")
                .When(x => x.SubmitAttempted);

            RuleFor(x => x.DestinationPath)
                .NotEmpty()
                .ValidPath()
                .MustNotBeIn(x => UsedPaths(x.InstanceName))
                .WithMessage(Validations.MSG_MUST_BE_UNIQUE, "Paths")
                .When(x => x.SubmitAttempted);

            RuleFor(x => x.DatabasePath)
                .NotEmpty()
                .ValidPath()
                .MustNotBeIn(x => UsedPaths(x.InstanceName))
                .WithMessage(Validations.MSG_MUST_BE_UNIQUE, "Paths")
                .When(x => x.SubmitAttempted);

            RuleFor(x => x.AuditForwarding)
                .NotNull().WithMessage(Validations.MSG_SELECTAUDITFORWARDING);

            RuleFor(x => x.ErrorForwarding)
                .NotNull().WithMessage(Validations.MSG_SELECTERRORFORWARDING);


            RuleFor(x => x.ErrorQueueName)
                .NotEmpty()
                .NotEqual(x => x.AuditQueueName).WithMessage(Validations.MSG_QUEUE_ALREADY_ASSIGNED, "Audit")
                .NotEqual(x => x.ErrorForwardingQueueName).WithMessage(Validations.MSG_QUEUE_ALREADY_ASSIGNED, "Error Forwarding")
                .NotEqual(x => x.AuditForwardingQueueName).WithMessage(Validations.MSG_QUEUE_ALREADY_ASSIGNED, "Audit Forwarding")
                .MustNotBeIn(x => UsedQueueNames(x.SelectedTransport, x.InstanceName, x.ConnectionString)).WithMessage(Validations.MSG_QUEUE_ALREADY_ASSIGNED)
                .When(x => x.SubmitAttempted);

            RuleFor(x => x.ErrorForwardingQueueName)
                .NotEmpty()
                .NotEqual(x => x.ErrorQueueName).WithMessage(Validations.MSG_UNIQUEQUEUENAME, "Error")
                .NotEqual(x => x.AuditQueueName).WithMessage(Validations.MSG_UNIQUEQUEUENAME, "Audit")
                .NotEqual(x => x.AuditForwardingQueueName).WithMessage(Validations.MSG_UNIQUEQUEUENAME, "Audit Forwarding")
                .MustNotBeIn(x => UsedQueueNames(x.SelectedTransport, x.InstanceName, x.ConnectionString)).WithMessage(Validations.MSG_QUEUE_ALREADY_ASSIGNED)
                .When(x => x.SubmitAttempted && x.ErrorForwarding.Value);

            RuleFor(x => x.AuditQueueName)
                .NotEmpty()
                .NotEqual(x => x.ErrorQueueName).WithMessage(Validations.MSG_UNIQUEQUEUENAME, "Error")
                .NotEqual(x => x.ErrorForwardingQueueName).WithMessage(Validations.MSG_UNIQUEQUEUENAME, "Error Forwarding")
                .NotEqual(x => x.AuditForwardingQueueName).WithMessage(Validations.MSG_UNIQUEQUEUENAME, "Audit Forwarding")
                .MustNotBeIn(x => UsedQueueNames(x.SelectedTransport, x.InstanceName, x.ConnectionString)).WithMessage(Validations.MSG_QUEUE_ALREADY_ASSIGNED)
                .When(x => x.SubmitAttempted);

            RuleFor(x => x.AuditForwardingQueueName)
                .NotEmpty()
                .NotEqual(x => x.ErrorQueueName).WithMessage(Validations.MSG_UNIQUEQUEUENAME, "Error")
                .NotEqual(x => x.AuditQueueName).WithMessage(Validations.MSG_UNIQUEQUEUENAME, "Audit")
                .NotEqual(x => x.ErrorForwardingQueueName).WithMessage(Validations.MSG_UNIQUEQUEUENAME, "Error Forwarding")
                .MustNotBeIn(x => UsedQueueNames(x.SelectedTransport, x.InstanceName, x.ConnectionString)).WithMessage(Validations.MSG_QUEUE_ALREADY_ASSIGNED)
                .When(x => x.SubmitAttempted && (x.AuditForwarding?.Value ?? false ));

            RuleFor(x => x.ConnectionString)
                .NotEmpty().WithMessage(Validations.MSG_THIS_TRANSPORT_REQUIRES_A_CONNECTION_STRING)
                .When(x => !string.IsNullOrWhiteSpace(x.SelectedTransport?.SampleConnectionString) && x.SubmitAttempted);
        }
    }
}