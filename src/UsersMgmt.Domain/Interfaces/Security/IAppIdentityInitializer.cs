using System;
using System.Collections.Generic;
using System.Text;

namespace UsersMgmt.Domain.Interfaces.Security
{
    public interface IAppIdentityInitializer
    {
        public void SeedData();
    }
}