using AutoMapper;
using UsersMgmt.App.Mappings;
using UsersMgmt.Domain.Entities;

namespace UsersMgmt.App.Security.Queries.GetAppUsers
{
    public class UserListItemDTO: IMapFrom<ApplicationUser>
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string UserRole { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ApplicationUser, UserListItemDTO>()
                .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Username, opt => opt.MapFrom(s => s.UserName));
        }
    }

}
