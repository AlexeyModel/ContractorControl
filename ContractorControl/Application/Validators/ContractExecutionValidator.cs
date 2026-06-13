using ContractorControl.Application.DTOs;
using FluentValidation;

namespace ContractorControl.Application.Validators;

public class SetStateDtoValidator : AbstractValidator<SetStateDto>
{
    public SetStateDtoValidator()
    {
        RuleFor(x => x.InstanceContractExecution).NotEmpty();
        RuleFor(x => x.StateName).NotEmpty();
        RuleFor(x => x.StateType).Must(t => t == "RUN" || t == "SUCCESS" || t == "FAILED")
            .WithMessage("StateType must be RUN, SUCCESS or FAILED.");
    }
}
