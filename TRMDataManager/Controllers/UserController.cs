using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using TRMDataManager.Library.DataAccess;
using TRMDataManager.Library.Models;
using TRMDataManager.Models;

namespace TRMDataManager.Controllers
{
    [Authorize]
    public class UserController : ApiController
    {
        [HttpGet]
        public UserModel GetById()
        {
            var id = RequestContext.Principal.Identity.GetUserId();
            UserData data = new UserData();

            return data.GetUserById(id).First();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("api/User/Admin/GetAllUsers")]
        public List<ApplicationUserModel> GetAll()
        {
            var applicationUserModels = new List<ApplicationUserModel>();

            using (var context = new ApplicationDbContext())
            {
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);

                var users = userManager.Users.ToList();
                var roles = context.Roles.ToList();

                foreach(var user in users)
                {
                    applicationUserModels.Add(new ApplicationUserModel
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Roles = user.Roles.ToDictionary(r => r.RoleId, r => roles.Single(m => m.Id == r.RoleId).Name)
                    });
                }
            }

            return applicationUserModels;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("api/User/Admin/GetAllRoles")]
        public Dictionary<string, string> GetAllRoles()
        {
            using (var context = new ApplicationDbContext())
            {
                return context.Roles.ToDictionary(r => r.Id, r => r.Name);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("api/User/Admin/AddRole")]
        public async Task AddRole(UserRolePairViewModel model)
        {
            using (var context = new ApplicationDbContext())
            {
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);

                await userManager.AddToRoleAsync(model.UserId, model.RoleName);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("api/User/Admin/RemoveRole")]
        public async Task RemoveRole(UserRolePairViewModel model)
        {
            using (var context = new ApplicationDbContext())
            {
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);

                await userManager.RemoveFromRoleAsync(model.UserId, model.RoleName);
            }
        }
    }
}
