
using Microsoft.AspNetCore.Mvc;
using webapi.Dtos.Auth;

namespace AuthWebApi.Services
{
    public interface IUserService
    {
        GetUserDto GetUser();
        GetUserInfoDto GetUserInfo();
    }
}
