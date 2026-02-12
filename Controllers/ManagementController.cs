using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DotNetBlueprint.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;


namespace DotNetBlueprint.Controllers
{
    [Authorize]
    public class ManagementController : Controller
    {
        private readonly AppDbContext _context;

        public ManagementController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Users()
        {
            // For now, any logged in user can see this, 
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
                
            return View(users);
        }
    }
}
