using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mossad_API.DAL;
using Mossad_API.Moddels;

namespace Mossad_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionsController : ControllerBase
    {
        private readonly MossadAPIDbContext _context;

        public MissionsController(MossadAPIDbContext context)
        {
            _context = context;
        }

        // Get all targets 

        //[HttpGet]
        //[Produces("application/json")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public IActionResult getAllMissions()
        //{
        //    return בהמשך לטיפול

        //}

        // Update starting position

        [HttpPut("{id}")]
        [Produces("application/json")]
        public IActionResult UpdateMissionStatus(int id, Location location)
        {
            Agent agent = _context.agents.FirstOrDefault(x => x.id == id);

            if (agent == null)
            {
                return StatusCode(
                400,
                new
                {
                    error = "The agent is invalid."
                });
            }

            agent._Location = location;

            _context.Update(agent);
            _context.SaveChanges();

            return Ok();
        }
    }
}
