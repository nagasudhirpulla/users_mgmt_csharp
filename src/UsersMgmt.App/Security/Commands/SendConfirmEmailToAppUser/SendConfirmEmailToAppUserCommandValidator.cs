using FluentValidation;

namespace UsersMgmt.App.Security.Commands.SendConfirmEmailToAppUser
{
    public class SendConfirmEmailToAppUserCommandValidator : AbstractValidator<SendConfirmEmailToAppUserCommand>
    {
        public SendConfirmEmailToAppUserCommandValidator()
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.BaseUrl).NotEmpty();
        }
    }
}
