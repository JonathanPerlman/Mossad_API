using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mossad_API.DAL;
using Mossad_API.Moddels;
using Mossad_API.Services;
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


        // Get all agents 

        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult getAllAgents()
        {
            return StatusCode(
                StatusCodes.Status200OK,
                new
                {
                    agents = _context.agents.Include(x => x._Location).ToList(),

                });

        }


        // Create Agent


        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult CreateAgent(CreateAgentRequest createAgentRequest)
        {
            //if (createAgentRequest == null) { }

            Agent agent = new Agent();
            agent.nickname = createAgentRequest.nickname;
            agent.photo_url = createAgentRequest.photo_url;
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
        public IActionResult PinAgent(int id, Location location)
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

            CreateMissionsForAgent(agent);


            _context.Update(agent);
            _context.SaveChanges();

            return Ok();
        }

        private void CreateMissionsForAgent(Agent agent)
        {

            List<Target> targets = _context.targets.Include(x => x._Location).Where(x => x.Status == TargetStatus.live).ToList();
            // להוסיף בדיקה האם יש מטרות 
            foreach (Target target in targets)
            {
                Double distance = Handler.GetDistance(target._Location, agent._Location);
                if (distance < 200)
                {
                    Mission mission = new Mission();
                    mission._agent = agent;
                    mission._target = target;
                    mission.Status = MissionStatus.Assigned;

                    _context.missions.Add(mission);
                    _context.SaveChanges();
                }
                else
                {
                    Console.WriteLine("The command is illegal");
                }

            }
        }



    }
}





