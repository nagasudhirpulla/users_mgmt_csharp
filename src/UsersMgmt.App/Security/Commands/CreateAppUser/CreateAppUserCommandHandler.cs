using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using UsersMgmt.Domain.Entities;

namespace UsersMgmt.App.Security.Commands.CreateAppUser
{
    public class CreateAppUserCommandHandler : IRequestHandler<CreateAppUserCommand, IdentityResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateAppUserCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> Handle(CreateAppUserCommand request, CancellationToken cancellationToken)
        {
            ApplicationUser user = new ApplicationUser { UserName = request.Username, Email = request.Email };
            IdentityResult result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                //TODO use logger here
                Console.WriteLine($"Created new account for {user.UserName} with id {user.Id}");
                // check if role string is valid
                bool isValidRole = SecurityConstants.GetRoles().Contains(request.UserRole);
                if (isValidRole)
                {
                    await _userManager.AddToRoleAsync(user, request.UserRole);
                    Console.WriteLine($"{request.UserRole} role assigned to new user {user.UserName} with id {user.Id}");
                }                
            }
            return result;
        }
    }
}
