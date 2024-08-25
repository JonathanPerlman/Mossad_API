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
    [Route("[controller]")]
    [ApiController]
    public class TargetsController : ControllerBase
    {
        private readonly MossadAPIDbContext _context;

        public TargetsController(MossadAPIDbContext context)
        {
            _context = context;
        }

         
        // פונקציה לקבלת רשימת המטרות
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTargets()
        {
            try
            {
                return StatusCode(
               StatusCodes.Status200OK,
               new
               {
                   targets = await _context.targets.Include(x => x._Location).ToListAsync()

               }) ;
            }
            catch
            {
                return StatusCode(500, new { error = "Connection failed" });

            }
        }

        // פונקציה ליצירת מטרה
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateTarget(CreateTargetRequest createTargetRequest)
        {
            try
            {
                if (createTargetRequest != null)
                {
                    Target target = new Target();
                    target.name = createTargetRequest.name;
                    target.position = createTargetRequest.position;
                    target.photo_url = createTargetRequest.photoUrl;
                    target.Status = TargetStatus.Live;
                    await _context.targets.AddAsync(target);
                    _context.SaveChangesAsync();

                    return StatusCode(
                    StatusCodes.Status201Created,
                    new { Id = target.id }
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


            

        // עדכון מקום התחלתי למטרה

        [HttpPut("{id}/pin")]
        [Produces("application/json")]
        public async Task <IActionResult>  PinTarget(int id, UpdateLocationRequest updateLocationRequest)
        {
            try
            {
                Target target = await _context.targets.Include(x => x._Location).FirstOrDefaultAsync(x => x.id == id);

                if (target == null)
                {
                    return StatusCode(
                    400,
                    new
                    {
                        error = "The target is invalid."
                    });
                }
                target._Location.x = updateLocationRequest.x;
                target._Location.y = updateLocationRequest.y;

                _context.Update(target);
                await _context.SaveChangesAsync();

                CreateMissionsForTarget(target);
                return Ok();
            }
            catch
            {
                return StatusCode(500, new { error = "Connection failed" });
            }
        }

        
        [HttpPut("{id}/move")]
        [Produces("application/json")]
        public async Task<IActionResult> MoveTarget(int id, MoveRequest moveRequest)
        {
            try
            {
                Target target = await _context.targets.Include(x => x._Location).FirstOrDefaultAsync(x => x.id == id);   

                if (target == null)
                {
                    return StatusCode(
                    400,
                    new
                    {
                        error = "The target is invalid."
                    });
                }
                Location newLocation = Handler.CalculateLocation(target._Location, moveRequest.direction);

                if(newLocation.x > 1000 || newLocation.y > 1000)
                {
                    return StatusCode(
                400,
                new
                {
                    error = "Agent cannot be moved  outside to the borders the matrix.",  location = target._Location
                }); ;
                }
                target._Location = newLocation;
                _context.Update(target);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(500, new { error = "Connection failed" });
            }
        }

        private async Task CreateMissionsForTarget(Target target)
        {
          
                List<Agent> agents = await _context.agents.Include(x => x._Location).Where(x => x.Status == AgentStatus.Dormant).ToListAsync();

                if (agents != null && agents.Count > 0)
                {
                    foreach (Agent agent in agents)
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















