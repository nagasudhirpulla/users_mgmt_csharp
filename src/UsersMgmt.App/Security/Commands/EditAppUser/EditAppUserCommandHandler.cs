using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UsersMgmt.Domain.Entities;

namespace UsersMgmt.App.Security.Commands.EditAppUser
{
    public class EditAppUserCommandHandler : IRequestHandler<EditAppUserCommand, List<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public EditAppUserCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<string>> Handle(EditAppUserCommand request, CancellationToken cancellationToken)
        {
            List<string> errors = new List<string>();
            ApplicationUser user = await _userManager.FindByIdAsync(request.Id);
            if (user == null)
            {
                errors.Add($"Unable to find user with id {request.Id}");
            }
            List<IdentityError> identityErrors = new List<IdentityError>();
            // change password if not null
            string newPassword = request.Password;
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                string passResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                IdentityResult passResetResult = await _userManager.ResetPasswordAsync(user, passResetToken, newPassword);
                if (passResetResult.Succeeded)
                {
                    Console.WriteLine("User password changed");
                }
                else
                {
                    identityErrors.AddRange(passResetResult.Errors);
                }
            }

            // change username if changed
            if (user.UserName != request.Username)
            {
                IdentityResult usernameChangeResult = await _userManager.SetUserNameAsync(user, request.Username);
                if (usernameChangeResult.Succeeded)
                {
                    Console.WriteLine("Username changed");

                }
                else
                {
                    identityErrors.AddRange(usernameChangeResult.Errors);
                }
            }

            // change email if changed
            if (user.Email != request.Email)
            {
                string emailResetToken = await _userManager.GenerateChangeEmailTokenAsync(user, request.Email);
                IdentityResult emailChangeResult = await _userManager.ChangeEmailAsync(user, request.Email, emailResetToken);
                if (emailChangeResult.Succeeded)
                {
                    Console.WriteLine("email changed");
                }
                else
                {
                    identityErrors.AddRange(emailChangeResult.Errors);
                }
            }

            // change user role if not present in user
            bool isValidRole = SecurityConstants.GetRoles().Contains(request.UserRole);
            List<string> existingUserRoles = (await _userManager.GetRolesAsync(user)).ToList();
            bool isRoleChanged = !existingUserRoles.Any(r => r == request.UserRole);
            if (isValidRole)
            {
                if (isRoleChanged)
                {
                    // remove existing user roles if any
                    await _userManager.RemoveFromRolesAsync(user, existingUserRoles);
                    // add new Role to user from VM
                    await _userManager.AddToRoleAsync(user, request.UserRole);
                }
            }
            foreach (IdentityError iError in identityErrors)
            {
                errors.Add(iError.Description);
            }
            return errors;
        }
    }
}
