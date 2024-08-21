using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mossad_API.DAL;
using Mossad_API.Moddels;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Text;

namespace Mossad_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentsController : ControllerBase
    {

        private readonly MossadAPIDbContext _context;

        public AgentsController(MossadAPIDbContext context)
        {
            _context = context;
        }


        // Create Agent


        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult CreateAgent(Agent agent)
        {

            agent.Status = AgentStatus.dormant;
            _context.agents.Add(agent);
            _context.SaveChanges();

            return StatusCode(
            StatusCodes.Status201Created,
            new { success = true, agentID = agent.id }
            );
        }

        // Update starting position

        [HttpPut("{id}/pin")]
        [Produces("application/json")]
        public IActionResult UpdateStartingPosition(int id, Location location)
        {
            Agent agent = _context.agents.FirstOrDefault(x => x.id == id);

            agent._Location = location;

            _context.Update(agent);
            _context.SaveChanges();

            return Ok();
        }
    }
}


