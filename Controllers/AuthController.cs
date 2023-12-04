using AuthWebApi.Dtos;
using AuthWebApi.Models;
using AuthWebApi.Services;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using webapi.Data;
using webapi.Dtos.Auth;
using webapi.Dtos.Order;

namespace AuthWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        //public static User user = new User();
        private readonly IConfiguration _configuration;
        private readonly DataContext _dbContext;
        private readonly IUserService _userService;

        public AuthController(IConfiguration configuration, DataContext dataContext, IUserService userService)
        {
            _configuration = configuration;
            _dbContext = dataContext;
            _userService = userService;
        }

        [HttpGet("user"), Authorize(Roles = "User")]
        public ActionResult<GetUserDto> GetUser()
        {
            return Ok(_userService.GetUser());
        }

        [HttpGet("userInfo"), Authorize(Roles = "User")]
        public ActionResult<GetUserInfoDto> GetUserInfo()
        {
            return Ok(_userService.GetUserInfo());
        }

        [HttpPut("userInfo"), Authorize(Roles = "User")]
        public async Task<ActionResult> updateUserInfo(UpdateUserInfoDto request)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.Id);
            if (user == null)
                return BadRequest("User not found");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return BadRequest("Incorrect password");

            if(request.FirstName != "" && request.FirstName != null)
            {
                user.FirstName = request.FirstName;
            }
            if (request.LastName != "" && request.LastName != null)
            {
                user.LastName = request.LastName;
            }
            if(request.NewPassword != "" && request.NewPassword != null)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            }

            await _dbContext.SaveChangesAsync();

            return Ok("User info updated");
        }

        [HttpGet("getOrders/{userId}"), Authorize(Roles = "User")]
        public async Task<ActionResult<List<GetOrderDto>>> GetOrdersByUserId(int userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var orders = await _dbContext.Orders.Where(o => o.UserId == user.Id && o.IsCompleted == true).OrderByDescending(o => o.CreatedAt).ToListAsync();
            if (orders == null || orders.Count == 0)
            {
                return BadRequest("Orders not found");
            }

            List<GetOrderDto> ordersDto = new List<GetOrderDto>();

            foreach (var order in orders)
            {
                var orderItems = await _dbContext.OrdersItems.Where(i => i.OrderId == order.Id).ToListAsync();
                var products = new List<GetOrderProductDto>();

                foreach (var item in orderItems)
                {
                    var prod = await _dbContext.Products.FindAsync(item.ProductId);
                    if (prod == null)
                    {
                        return BadRequest("Some item not found");
                    }
                    products.Add(new GetOrderProductDto(prod.Id, prod.Name, item.Quantity));
                }
                ordersDto.Add(new GetOrderDto(user.Id, order.Name, order.Total, order.CreatedAt, products));
            }

            return Ok(ordersDto);
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterUserDto request)
        {
            // Validate request format
            if (ModelState.IsValid)
            {
                // Check if username already exists
                if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
                    return BadRequest("Username already exists.");                

                // Create new user
                User newUser = new() {
                    Username = request.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    StripeCustomerId = request.StripeCustomerId
                };

                // Add new user to db
                try {
                    _dbContext.Users.Add(newUser);
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception e) {
                   return BadRequest(e.Message);
                }
                
                return Ok("Registered successfuly.");
            }
            return BadRequest("Model state invalid.");
        }


        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginUserDto request)
        {
            // Validate request format
            if (ModelState.IsValid)
            {
                // Get user by username from db
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

                // Check if username doesn't exist
                if (user == null)
                    return BadRequest("Username doesn't exist");

                // Check if password is correct
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                    return BadRequest("Wrong passowrd");

                // Create Token
                var token = GenerateJwtToken(user);

                // Create Refresh Token
                var refreshToken = GenerateRefreshToken();
                SetRefreshToken(refreshToken, user);

                // Add refresh token to user in db
                user.RefreshToken = refreshToken.Token;
                try {
                    _dbContext.Users.Update(user);
                    await _dbContext.SaveChangesAsync();
                }
                catch(Exception e) {
                    return BadRequest(e.Message);
                }

                return Ok(token);   // Return access token (Logged in)          
            }
            return BadRequest("Model state invalid.");

        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken()  // string token     // task<actionresult<string>>>
        {
            // Get refresh token
            string? refreshToken = Request.Cookies["refreshToken"];
            //string? token = myToken;

            // Get principal from (expired) token
            //ClaimsPrincipal principal = GetPrincipalFromExpiredToken(token);    // token

            //if (principal == null)
            //    return BadRequest("Invalid access or refresh token");

            // Get username from principal
            //var username = principal.Identity?.Name;


            // Get user by username from principal
            //var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

            // Check if user exists, if refresh token is same as user, if refresh token expired
            //if (user == null || user.RefreshToken != refreshToken || user.TokenExpires <= DateTime.Now)
            //    return BadRequest("Invalid access token or refresh token");

            // ********** Get user by refresh token ********** ????
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (refreshToken == null || user == null || user.TokenExpires <= DateTime.Now)
                return BadRequest("Invalid refresh token");
            // **********************************************
      
            // Create new token & new refresh token
            string newToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();
            SetRefreshToken(newRefreshToken, user);

            // Update refresh token of user in db
            try
            {
                user.RefreshToken = newRefreshToken.Token;
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok(newToken);
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke(string username)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return BadRequest("Invalid username.");

            Response.Cookies.Delete("refreshToken");    //
            user.RefreshToken = null;
            user.TokenCreated = null;
            user.TokenExpires = null;
            await _dbContext.SaveChangesAsync();

            return Ok("Refresh token revoked");
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = false,   // false only when creaing principal to refresh token
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("Jwt:Key").Value!)),
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512Signature, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        private RefreshToken GenerateRefreshToken()
        {
            // Create new refresh token
            var refreshToken = new RefreshToken
            {
                Token = getUniqueToken(),
                Expires = DateTime.Now.AddDays(7)       // refresh token: 7 days
            };
            return refreshToken;

            string getUniqueToken()     // generate unique refresh token (create new one if already exists)
            {
                var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

                if (_dbContext.Users.Any(u => u.RefreshToken == token))
                {
                    return getUniqueToken();
                }
                return token;
            }
        }

        private void SetRefreshToken(RefreshToken newRefreshToken, User user)
        {
            // Create new cookie options
            var cookieOptions = new CookieOptions
            { 
                HttpOnly = true,
                Expires = newRefreshToken.Expires,
            };

            // Store new refresh token in cookie
            Response.Cookies.Append("refreshToken", newRefreshToken.Token!, cookieOptions);

            // Set user values from db
            user.RefreshToken = newRefreshToken.Token;
            user.TokenCreated = newRefreshToken.Created;
            user.TokenExpires = newRefreshToken.Expires;            
        }

        private string GenerateJwtToken(User user)
        {
            // Create list of claims
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Email, user.Username),
                new Claim(ClaimTypes.Role, "User")
            };

            // Create security key with key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("Jwt:Key").Value!));

            // Create credendials
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Create new token
            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(15),   // jwt token: 15 minutes
                    signingCredentials: creds
                ); ;

            // Serialize token to a string
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);


            // ************ ??? ************************
            //var cookieOptions = new CookieOptions
            //{
            //    HttpOnly = true,
            //    Expires = token.ValidTo,
            //};
            //Response.Cookies.Append("token", jwt, cookieOptions);

            return jwt;
        }
    }
}
