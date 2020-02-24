using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UsersMgmt.App.Security;
using UsersMgmt.Domain.Entities;
using UsersMgmt.Web.Extensions;
using UsersMgmt.Web.Models;

namespace UsersMgmt.Web.Controllers
{
    [Authorize(Roles = SecurityConstants.AdminRoleString)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;
        private readonly IdentityInit _identityInit;
        public UsersController(UserManager<ApplicationUser> userManager, ILogger<UsersController> logger, IdentityInit identityInit)
        {
            // acquire user manager via dependency injection
            _userManager = userManager;
            _logger = logger;
            _identityInit = identityInit;
        }

        public async Task<IActionResult> Index()
        {
            UserListVM vm = new UserListVM();
            vm.Users = new List<UserListItemVM>();
            // get the list of users
            List<ApplicationUser> users = await _userManager.Users.ToListAsync();
            foreach (ApplicationUser user in users)
            {
                // get user is of admin role
                //bool isSuperAdmin = (await _userManager.GetRolesAsync(user)).Any(r => r == SecurityConstants.AdminRoleString);
                // todo make identity init  singleton service like email config so as to avoid raw strings usage
                bool isSuperAdmin = (user.UserName == _identityInit.AdminUserName);
                if (!isSuperAdmin)
                {
                    // add user to vm only if not admin
                    string userRole = "";
                    IList<string> existingRoles = await _userManager.GetRolesAsync(user);
                    if (existingRoles.Count > 0)
                    {
                        userRole = existingRoles.ElementAt(0);
                    }
                    vm.Users.Add(new UserListItemVM
                    {
                        UserId = user.Id,
                        Username = user.UserName,
                        Email = user.Email,
                        UserRole = userRole
                    });
                }

            }
            return View(vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["UserRole"] = new SelectList(SecurityConstants.GetRoles());
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateVM vm)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser { UserName = vm.Username, Email = vm.Email };
                IdentityResult result = await _userManager.CreateAsync(user, vm.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Created new account for {user.UserName} with id {user.Id}");
                    // check if role string is valid
                    bool isValidRole = SecurityConstants.GetRoles().Contains(vm.UserRole);
                    if (isValidRole)
                    {
                        await _userManager.AddToRoleAsync(user, vm.UserRole);
                        _logger.LogInformation($"{vm.UserRole} role assigned to new user {user.UserName} with id {user.Id}");
                    }

                    return RedirectToAction(nameof(Index)).WithSuccess($"Created new user {user.UserName} {(isValidRole ? $"with role {vm.UserRole}" : "")}");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            ViewData["UserRole"] = new SelectList(SecurityConstants.GetRoles());
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            IList<string> existingUserRoles = (await _userManager.GetRolesAsync(user));
            string userRole = SecurityConstants.GuestRoleString;
            if (existingUserRoles.Count > 0)
            {
                userRole = existingUserRoles.ElementAt(0);
            }
            UserEditVM vm = new UserEditVM()
            {
                Email = user.Email,
                Username = user.UserName,
                UserRole = userRole
            };
            ViewData["UserRole"] = new SelectList(SecurityConstants.GetRoles());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserEditVM vm)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return NotFound();
                }

                ApplicationUser user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                List<IdentityError> identityErrors = new List<IdentityError>();
                // change password if not null
                string newPassword = vm.Password;
                if (newPassword != null)
                {
                    string passResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                    IdentityResult passResetResult = await _userManager.ResetPasswordAsync(user, passResetToken, newPassword);
                    if (passResetResult.Succeeded)
                    {
                        _logger.LogInformation("User password changed");
                    }
                    else
                    {
                        identityErrors.AddRange(passResetResult.Errors);
                    }
                }

                // change username if changed
                if (user.UserName != vm.Username)
                {
                    IdentityResult usernameChangeResult = await _userManager.SetUserNameAsync(user, vm.Username);
                    if (usernameChangeResult.Succeeded)
                    {
                        _logger.LogInformation("Username changed");

                    }
                    else
                    {
                        identityErrors.AddRange(usernameChangeResult.Errors);
                    }
                }

                // change email if changed
                if (user.Email != vm.Email)
                {
                    string emailResetToken = await _userManager.GenerateChangeEmailTokenAsync(user, vm.Email);
                    IdentityResult emailChangeResult = await _userManager.ChangeEmailAsync(user, vm.Email, emailResetToken);
                    if (emailChangeResult.Succeeded)
                    {
                        _logger.LogInformation("email changed");
                    }
                    else
                    {
                        identityErrors.AddRange(emailChangeResult.Errors);
                    }
                }

                // change user role if not present in user
                bool isValidRole = SecurityConstants.GetRoles().Contains(vm.UserRole);
                List<string> existingUserRoles = (await _userManager.GetRolesAsync(user)).ToList();
                bool isRoleChanged = !existingUserRoles.Any(r => r == vm.UserRole);
                if (isValidRole)
                {
                    if (isRoleChanged)
                    {
                        // remove existing user roles if any
                        await _userManager.RemoveFromRolesAsync(user, existingUserRoles);
                        // add new Role to user from VM
                        await _userManager.AddToRoleAsync(user, vm.UserRole);
                    }
                }

                // check if we have any errors and redirect if successful
                if (identityErrors.Count == 0)
                {
                    _logger.LogInformation("User edit operation successful");

                    return RedirectToAction(nameof(Index)).WithSuccess("User Editing done");
                }

                AddErrors(identityErrors);
            }

            // If we got this far, something failed, redisplay form
            ViewData["UserRole"] = new SelectList(SecurityConstants.GetRoles());
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            UserDeleteVM vm = new UserDeleteVM()
            {
                Email = user.Email,
                Username = user.UserName,
                UserId = user.Id
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(UserDeleteVM vm)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(vm.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                IdentityResult result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User deleted successfully");

                    return RedirectToAction(nameof(Index)).WithSuccess("User deletion done");
                }

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(vm);
        }

        // helper function
        private void AddErrors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // helper function
        private void AddErrors(IEnumerable<IdentityError> errs)
        {
            foreach (IdentityError error in errs)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}