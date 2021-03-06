using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TRMApi.Data;
using TRMApi.Models;
using TRMDataManager.Library.DataAccess;
using TRMDataManager.Library.Models;

namespace TRMApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserData _userData;

        public UserController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, IUserData userData)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _userData = userData;
        }

        [HttpGet]
        public UserModel GetById()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return _userData.GetUserById(id).First();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("Admin/GetAllUsers")]
        public List<ApplicationUserModel> GetAll()
        {
            var applicationUserModels = new List<ApplicationUserModel>();

            var users = _dbContext.Users.ToList();
            var userRoles = from ur in _dbContext.UserRoles
                            join r in _dbContext.Roles on ur.RoleId equals r.Id
                            select new { ur.UserId, ur.RoleId, r.Name };

            foreach (var user in users)
            {
                applicationUserModels.Add(new ApplicationUserModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Roles = userRoles.Where(x => x.UserId == user.Id).ToDictionary(k => k.RoleId, v => v.Name)
                }); 
            }

            return applicationUserModels;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("Admin/GetAllRoles")]
        public Dictionary<string, string> GetAllRoles()
        {
            return _dbContext.Roles.ToDictionary(r => r.Id, r => r.Name);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("Admin/AddRole")]
        public async Task AddRole(UserRolePairViewModel model)
        {
            var user = await _dbContext.Users.FindAsync(model.UserId);
            await _userManager.AddToRoleAsync(user, model.RoleName);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("Admin/RemoveRole")]
        public async Task RemoveRole(UserRolePairViewModel model)
        {
            var user = await _dbContext.Users.FindAsync(model.UserId);
            await _userManager.RemoveFromRoleAsync(user, model.RoleName);
        }
    }
}
