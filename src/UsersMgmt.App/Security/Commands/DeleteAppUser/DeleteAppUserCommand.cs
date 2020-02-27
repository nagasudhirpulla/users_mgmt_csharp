using MediatR;
using System.Collections.Generic;

namespace UsersMgmt.App.Security.Commands.DeleteAppUser
{
    public class DeleteAppUserCommand : IRequest<List<string>>
    {
        public string Id { get; set; }
    }
}
