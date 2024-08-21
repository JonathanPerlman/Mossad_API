using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mossad_API.DAL;
using Mossad_API.Moddels;
using Newtonsoft.Json;
using System.Text;

namespace Mossad_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TargetsController : ControllerBase
    {
        private readonly MossadAPIDbContext _context;

        public TargetsController(MossadAPIDbContext context)
        {
            _context = context;
        }

        // Create Target


        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult CreateTarget(Target target)
        {
            var jsonContent = JsonConvert.SerializeObject(target);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");


            target.Status = TargetStatus.live;
            _context.targets.Add(target);
            _context.SaveChanges();

            return StatusCode(
            StatusCodes.Status201Created,
            new { success = true, targetID = target.id }
            );
        }
    }
}
