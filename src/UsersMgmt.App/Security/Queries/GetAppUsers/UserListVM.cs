using System.Collections.Generic;

namespace UsersMgmt.App.Security.Queries.GetAppUsers
{
    public class UserListVM
    {
        public IList<UserListItemDTO> Users { get; set; }
    }

}
