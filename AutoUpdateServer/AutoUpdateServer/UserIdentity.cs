using AutoUpdateServer.Enum;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer
{
    public class UserIdentity : IUserIdentity
    {
        public UserIdentity(string userName, Permission permission) :
            this(userName, permission, new List<string>())
        {
        }
        public UserIdentity(string userName, Permission permission, IEnumerable<string> claims)
        {
            this.UserName = userName;
            this.Permission = permission;
            this.Claims = claims;
        }

        public IEnumerable<string> Claims
        {
            get; private set;
        }

        public string UserName
        {
            get; private set;
        }

        public Permission Permission
        {
            get; private set;
        }
    }
}