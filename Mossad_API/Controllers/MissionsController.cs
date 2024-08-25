using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mossad_API.DAL;
using Mossad_API.Moddels;
using Mossad_API.Services;
using System;

namespace Mossad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MissionsController : ControllerBase
    {
        private readonly MossadAPIDbContext _context;

        public MissionsController(MossadAPIDbContext context)
        {
            _context = context;
        }

       // לטיפול בהמשך יחד עם הMVC
        //[HttpGet]
        //[Produces("application/json")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public IActionResult GetSuggestionMissions ()
        //{

        //}

        // Update starting position


        [HttpPut("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> UpdateMissionStatus(int id, UpdateMissionRequest updateMissionRequest)
        {
            try
            {
                Mission mission = await _context.missions.Include(x => x._agent).ThenInclude(x => x._Location).Include(x => x._target).ThenInclude(x => x._Location).FirstOrDefaultAsync(x => x.Id == id);
                                  
                if (mission == null)
                {
                    return StatusCode(
                    400,
                    new
                    {
                        error = "The mission is invalid."
                    });
                }
                
                if (mission.Status == MissionStatus.Ended)
                {
                    return StatusCode(400, new { error = "The task has ended and therefore cannot be updated" });
                }
                Double distance = Handler.GetDistance(mission._target._Location, mission._agent._Location);
                
                if(distance > 200)
                {
                    _context.missions.Remove(mission);
                    await _context.SaveChangesAsync();

                    return StatusCode(400, new { error = "Can't be assigned to the target because of the distance" });
                }
                mission.Status = MissionStatus.Assigned;
                mission.TimeLeft = distance / 5;
                mission._agent.Status = AgentStatus.InActivity;

                _context.missions.Update(mission);
                await _context.SaveChangesAsync();

                await DeleteMissions(mission._agent.id, mission._target.id);

                return Ok();
            }
            catch
            {
                return BadRequest();
            }
      
        }

        [HttpPost("update")]
        [Produces("application/json")]

        public async Task  UpdateMissions()
        {
            //List<Mission> missionWithStatusAssigned = await _context.missions.Include(x => x._agent).ThenInclude(x => x._Location).Where(x => x.Status == MissionStatus.Assigned).Include(x => x._target).ThenInclude(x => x._Location).ToListAsync();

            List<Mission> missions = await _context.missions.Include(x => x._target).Include(x => x._agent).ToListAsync();

            foreach (Mission mission in missions)
            {
                Target target = await _context.targets.Include(x => x._Location).FirstOrDefaultAsync(x => x.id == mission._target.id);

                Agent agent = await _context.agents.Include(x => x._Location).FirstOrDefaultAsync(x => x.id == mission._agent.id);
                
                Double distance =  Handler.GetDistance(mission._target._Location, mission._agent._Location);


                if(distance > 200)
                {
                    _context.missions.Remove(mission);
                    await _context.SaveChangesAsync();

                }
                string direction = Handler.CalculateDirection(mission._agent, mission._target);

                Location newLocation =  Handler.CalculateLocation(mission._agent._Location, direction);

                Double newDistance = Handler.GetDistance(newLocation, mission._target._Location);

                Double timeLeft =  newDistance / 5;

                if(timeLeft == 0)
                {
                    KillTarget(mission);
                }
                else
                {
                mission.TimeLeft = timeLeft;
                _context.missions.Update(mission);
                await _context.SaveChangesAsync();

                }
            }
        }

        private async Task DeleteMissions(int agendId, int targetId)
        {
            List<Mission> deleteMissionsForAgent = await _context.missions.Include(x => x._agent).Where(x => x._agent.id == agendId).ToListAsync();
            List<Mission> deleteMissionsForTarget = await _context.missions.Include(x => x._target).Where(x => x._target.id == agendId).ToListAsync();

            _context.missions.RemoveRange(deleteMissionsForAgent);
            _context.missions.RemoveRange(deleteMissionsForTarget);


            await _context.SaveChangesAsync();  
        }

        private async void KillTarget(Mission mission)
        {
            mission._target.Status = TargetStatus.Eliminated;
            mission._agent.Status = AgentStatus.Dormant;
            mission.Status = MissionStatus.Ended;

            _context?.missions.Update(mission);

            await _context.SaveChangesAsync();
        }
    }
}




