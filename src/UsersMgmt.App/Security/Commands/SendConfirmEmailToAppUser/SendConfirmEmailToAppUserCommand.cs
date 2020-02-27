using MediatR;
using Microsoft.AspNetCore.Identity;
using UsersMgmt.Domain.Entities;

namespace UsersMgmt.App.Security.Commands.SendConfirmEmailToAppUser
{
    public class SendConfirmEmailToAppUserCommand : IRequest<bool>
    {
        public string Username { get; set; }
        public string BaseUrl { get; set; }
    }
}
