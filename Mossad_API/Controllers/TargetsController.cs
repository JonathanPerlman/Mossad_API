using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mossad_API.DAL;
using Mossad_API.Moddels;
using Mossad_API.Services;
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

        // Get all targets 

        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult getAllTargets()
        {
            return StatusCode(
                StatusCodes.Status200OK,
                new
                {
                    targets = _context.targets.Include(x => x._Location).ToList(),

                });

        }

        // Create Target


        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult CreateTarget(CreateTargetRequest createTargetRequest)
        {
            //if (createTargetRequest == null) { }

            Target target = new Target();
            target.name = createTargetRequest.name;
            target.position = createTargetRequest.position;
            target.photo_url = createTargetRequest.photo_url;
            target.Status = TargetStatus.live;
            _context.targets.Add(target);
            _context.SaveChanges();

            return StatusCode(
            StatusCodes.Status201Created,
            new { success = true, targetID = target.id }
            );
        }

        // Update starting position

        [HttpPut("{id}/pin")]
        [Produces("application/json")]
        public IActionResult PinTarget(int id, Location location)
        {
            Target target = _context.targets.FirstOrDefault(x => x.id == id);

            if (target == null)
            {
                return StatusCode(
                400,
                new
                {
                    error = "The target is invalid."
                });
            }
            target._Location = location;

            CreateMissionsForTarget(target);

            _context.Update(target);
            _context.SaveChanges();

            return Ok();
        }


        private void CreateMissionsForTarget(Target target)
        {
            List<Agent> agents = _context.agents.Include(x => x._Location).Where(x => x.Status == AgentStatus.dormant).ToList();
            // להוסיף בדיקה האם יש סוכנים
            foreach (Agent agent in agents)
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





