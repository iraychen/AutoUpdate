using AutoUpdateServer.Model;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;
using System;

namespace AutoUpdateServer
{
    public class UserMapper : IUserMapper
    {
        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            UserModel userRecord = context.Request.Session[identifier.ToString()] as UserModel;
            return userRecord == null ? null : new UserIdentity(userRecord.Name,userRecord.Permission);
        }
    }
}