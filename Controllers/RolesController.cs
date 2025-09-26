//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.AspNetCore.Mvc;

//namespace ProyetoSetilPF.Controllers
//{
//    [Authorize(Roles = "Admin")]
//    public class RolesController : Controller
//    {
//        private readonly RoleManager<IdentityRole> _roleManager;

//        public RolesController(RoleManager<IdentityRole> roleManager)
//        {
//            _roleManager = roleManager;
//        }

//        public IActionResult Index()
//        {
//            var roles = _roleManager.Roles.ToList();
//            return View(roles);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create(string name)
//        {
//            if (!string.IsNullOrWhiteSpace(name))
//            {
//                var exists = await _roleManager.RoleExistsAsync(name);
//                if (!exists)
//                {
//                    var result = await _roleManager.CreateAsync(new IdentityRole(name));
//                    if (result.Succeeded)
//                        return RedirectToAction(nameof(Index));
//                    ModelState.AddModelError("", string.Join(", ", result.Errors.Select(e => e.Description)));
//                }
//            }
//            return View("Index", _roleManager.Roles);
//        }
//    }
//}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace ProyetoSetilPF.Controllers
{
    [Authorize(Roles = "Admin")] // Solo Admin puede crear roles
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (!await _roleManager.RoleExistsAsync(name))
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(name));
                    if (result.Succeeded)
                        return RedirectToAction(nameof(Index));

                    ModelState.AddModelError("", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            var roles = _roleManager.Roles.ToList();
            return View("Index", roles);
        }
    }
}