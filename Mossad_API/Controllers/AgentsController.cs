using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mossad_API.DAL;
using Mossad_API.Moddels;
using Mossad_API.Services;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Mossad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AgentsController : ControllerBase
    {

        private readonly MossadAPIDbContext _context;

        public AgentsController(MossadAPIDbContext context)
        {
            _context = context;
        }


        // פונקציה לקבלת רשימת הסוכנים 

        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAgents()
        {
            try
            {
                return StatusCode(
               StatusCodes.Status200OK,
               new
               {
                   Agents = await _context.agents.Include(x => x._Location).ToListAsync()

               });
            }
            catch
            {
                return StatusCode(500, new { error = "Connection failed" });

            }

        }

        // פונקציה ליצירת סוכן
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task <IActionResult> CreateAgent(CreateAgentRequest createAgentRequest)
        {
            try
            {
                if (createAgentRequest != null) 
                {
                    Agent agent = new Agent();
                    agent.nickname = createAgentRequest.nickname;
                    agent.photo_url = createAgentRequest.photoUrl;  
                    agent.Status = AgentStatus.Dormant;

                    await _context.agents.AddAsync(agent);
                    await _context.SaveChangesAsync();

                    return StatusCode(
                    StatusCodes.Status201Created,
                    new { Id = agent.id }
                    );
                }
                else
                {
                    return StatusCode(400, new { error = "Invalid request" });

                }
            }
            catch
            {
                return StatusCode(500, new { error = "Connection failed" });

            }

        }

             

        // עדכון מקום התחלתי לסוכן

        [HttpPut("{id}/pin")]
        [Produces("application/json")]
        public async Task<IActionResult> PinAgent(int id, UpdateLocationRequest updateLocationRequest)
        {
            try 
            {
                Agent agent = await _context.agents.Include(x => x._Location).FirstOrDefaultAsync(x => x.id == id);

                if (agent == null)
                {
                    return StatusCode(
                    400,
                    new
                    {
                        error = "The agent is invalid."
                    });
                }

                agent._Location = new Location();
                agent._Location.x = updateLocationRequest.x;
                agent._Location.y = updateLocationRequest.y;

                _context.Update(agent);
                await _context.SaveChangesAsync();

                CreateMissionsForAgent(agent);

                return Ok();
            }
            catch
            {
                return StatusCode(500, new { error = "Connection failed" });
            }
        }

        [HttpPut("{id}/move")]
        [Produces("application/json")]
        public async Task<IActionResult> MoveAgent(int id, MoveRequest moveRequest)
        {
            try
            {
                Agent agent = await _context.agents.Include(x => x._Location).FirstOrDefaultAsync(x => x.id == id);
                if (agent == null)
                {
                    return StatusCode(
                    400,
                    new
                    {
                        error = "The target is invalid."
                    });
                }
                if (agent.Status == AgentStatus.InActivity)
                {
                    return StatusCode(400, new { error = "Agent cannot be moved  in action" });
                }
                Location newLocation = Handler.CalculateLocation(agent._Location, moveRequest.direction);

                if (newLocation.x > 1000 || newLocation.y > 1000)
                {
                    return StatusCode(
                400,
                new
                {
                    error = "Agent cannot be moved  outside to the borders the matrix.",
                    location = agent._Location
                }); ;
                }
                agent._Location = newLocation;
                _context.Update(agent);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(500, new { error = "Connection failed" });
            }
        }

        private async Task CreateMissionsForAgent(Agent agent)
        {

            List<Target> targets = await _context.targets.Include(x => x._Location).Where(x => x.Status == TargetStatus.Live).ToListAsync();

            if (targets != null && targets.Count > 0)
            {
                foreach (Target target in targets)
                {
                    if(target._Location != null)
                    {
                        Double distance = Handler.GetDistance(target._Location, agent._Location);
                        if (distance < 200)
                        {
                            Mission mission = new Mission();
                            mission._agent = agent;
                            mission._target = target;
                            mission.Status = MissionStatus.Suggestion;

                            _context.missions.Add(mission);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
        }
    }
}
                 









     








