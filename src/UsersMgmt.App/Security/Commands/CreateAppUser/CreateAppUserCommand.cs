using MediatR;
using Microsoft.AspNetCore.Identity;

namespace UsersMgmt.App.Security.Commands.CreateAppUser
{
    public class CreateAppUserCommand : IRequest<IdentityResult>
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string UserRole { get; set; }
        public string BaseUrl { get; set; }
    }
}
