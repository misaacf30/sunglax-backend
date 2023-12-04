using Azure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using webapi.Data;
using webapi.Dtos.Auth;

namespace AuthWebApi.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly DataContext _dbContext;

        public UserService(IHttpContextAccessor contextAccessor, DataContext dataContext)
        {
            _contextAccessor = contextAccessor;
            _dbContext = dataContext;
        }

        public GetUserDto GetUser()
        {
            var username = string.Empty;
            var firstName = string.Empty;
            if (_contextAccessor.HttpContext is not null)
            {
                username = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
                firstName = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.GivenName);
            }

            var user = new GetUserDto
            {
                Username = username,
                FirstName = firstName,
            };
            return user;
        }

        public GetUserInfoDto GetUserInfo()
        {
            var username = string.Empty;
            if (_contextAccessor.HttpContext is not null)
            {
                username = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
            }

            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return new GetUserInfoDto();
            }

            var userInfo = new GetUserInfoDto
            {
                Id = user.Id,
                Username = username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                StripeCustomerId = user.StripeCustomerId
            };
            return userInfo;
        }


    }
}
