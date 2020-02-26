using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UsersMgmt.Domain.Entities;
using AutoMapper;

namespace UsersMgmt.App.Security.Queries.GetAppUsers
{
    public class GetAppUsersListQuery : IRequest<UserListVM>
    {
        public class GetAppUsersListQueryHandler : IRequestHandler<GetAppUsersListQuery, UserListVM>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IdentityInit _identityInit;
            private readonly IMapper _mapper;

            public GetAppUsersListQueryHandler(UserManager<ApplicationUser> userManager, IdentityInit identityInit, IMapper mapper)
            {
                _userManager = userManager;
                _identityInit = identityInit;
                _mapper = mapper;
            }

            public async Task<UserListVM> Handle(GetAppUsersListQuery request, CancellationToken cancellationToken)
            {
                UserListVM vm = new UserListVM();
                vm.Users = new List<UserListItemDTO>();
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
                        UserListItemDTO uDTO = _mapper.Map<UserListItemDTO>(user);
                        uDTO.UserRole = userRole;
                        vm.Users.Add(uDTO);
                    }

                }
                return vm;
            }
        }
    }
}
