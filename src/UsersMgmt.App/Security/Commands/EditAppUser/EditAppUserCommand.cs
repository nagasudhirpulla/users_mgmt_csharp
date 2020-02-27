using AutoMapper;
using MediatR;
using System.Collections.Generic;
using UsersMgmt.App.Mappings;
using UsersMgmt.App.Security.Queries.GetAppUsers;

namespace UsersMgmt.App.Security.Commands.EditAppUser
{
    public class EditAppUserCommand : IRequest<List<string>>, IMapFrom<UserDTO>
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string UserRole { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UserDTO, EditAppUserCommand>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.UserId));
        }
    }
}
