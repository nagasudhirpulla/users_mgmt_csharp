using FluentValidation;

namespace UsersMgmt.App.Security.Commands.EditAppUser
{
    public class EditAppUserCommandValidator : AbstractValidator<EditAppUserCommand>
    {
        public EditAppUserCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.Email).EmailAddress().NotEmpty();
            RuleFor(x => x.UserRole).NotEmpty();
            RuleFor(x => x.Password)
                .Equal(x => x.ConfirmPassword).WithMessage("Password and confirmation password do not match.");
        }
    }
}
