namespace ServiceControl.Config.UI.InstanceEdit
{
    using FluentValidation;
    using Validation;

    public class InstanceEditViewModelValidator : SharedInstanceEditorViewModelValidator<InstanceEditViewModel>
    {
        public InstanceEditViewModelValidator()
        {
            RuleFor(x => x.SelectedTransport).NotEmpty();
        }
    }
}