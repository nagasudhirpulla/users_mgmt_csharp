using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UsersMgmt.Domain.Entities;

namespace UsersMgmt.App.Security.Commands.DeleteAppUser
{
    public class DeleteAppUserCommandHandler : IRequestHandler<DeleteAppUserCommand, List<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteAppUserCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<string>> Handle(DeleteAppUserCommand request, CancellationToken cancellationToken)
        {
            List<string> errors = new List<string>();
            ApplicationUser user = await _userManager.FindByIdAsync(request.Id);
            if (user == null)
            {
                errors.Add($"User not found with id {request.Id}");
            }

            IdentityResult result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                foreach (IdentityError err in result.Errors)
                {
                    errors.Add(err.Description);
                }
            }

            return errors;
        }
    }
}
