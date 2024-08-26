using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mossad_API.DAL;
using Mossad_API.Moddels;
using Mossad_API.Moddels.DBModdels;
using Mossad_API.Moddels.Enums;
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

               });
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
                    // עדכון המטרה בפרמטרים הבאים מהסימולציה
                    Target target = new Target();
                    target.name = createTargetRequest.name;
                    target.position = createTargetRequest.position;
                    target.photo_url = createTargetRequest.photoUrl;
                    target.Status = TargetStatus.Live;
                    // הוספת הנתונים ושמירתם 
                    await _context.targets.AddAsync(target);
                    await _context.SaveChangesAsync();
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
        public async Task<IActionResult> PinTarget(int id, UpdateLocationRequest updateLocationRequest)
        {
            try
            {
                // שליפה של  סוכן המבוקש + המיקום 
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
                // יצירת מופע למיקום + עדכון הנתונים 
                target._Location = new Location();

                target._Location.X = updateLocationRequest.x;
                target._Location.Y = updateLocationRequest.y;
                // עדכון הנתונים בDB ושמירתם
                _context.Update(target);
                await _context.SaveChangesAsync();
                // שליחת המטרה לפונקציה שמייצרת משימה עבורה
                await CreateMissionsForTarget(target);
                return Ok();
            }
            catch
            {
                return StatusCode(500, new { error = "Connection failed" });
            }
        }



        // פונקציה להזזת מטרה
        [HttpPut("{id}/move")]
        [Produces("application/json")]
        public async Task<IActionResult> MoveTarget(int id, MoveRequest moveRequest)
        {
            try
            {
                // שליפה מהדאטה-בייס של המטרה + המיקום
                // לפי הId שהתקבל
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
                // קריאה לפונקציה שמזיזה את הסוכן לפי מיקומו והכיוון שהתקבל 
                Location newLocation = Handler.CalculateLocation(target._Location, moveRequest.direction);
                // אם המיקום החדש מחוץ לגבולות המטריצה חוזרת שגיאה מתאימה + המיקום 

                if (newLocation.X > 1000 || newLocation.Y > 1000)
                {
                    return StatusCode(
                400,
                new
                {
                    error = "Agent cannot be moved  outside to the borders the matrix.",
                    location = target._Location
                }); ;
                }
                // עדכון המיקום החדש + עדכון בדאטה + שמירה
                target._Location = newLocation;
                _context.Update(target);
                await _context.SaveChangesAsync();

                await CreateMissionsForTarget(target);

                return Ok();
            }
            catch
            {
                return StatusCode(500, new { error = "Connection failed" });
            }
        }
       
        // פונקציה ליצירת משימה עבור מטרה
        private async Task CreateMissionsForTarget(Target target)
        {
            // "שליפה מבסיס הנתונים לתוך ליסט את כל הסוכנים + המיקומים שלהם כשהסטטוס הוא "רדום

            List<Agent> agents = await _context.agents.Include(x => x._Location).Where(x => x.Status == AgentStatus.Dormant).ToListAsync();

            if (agents != null && agents.Count > 0)
            {
                foreach (Agent agent in agents)
                {// בדיקה אם המיקומים של הסוכן והמטרה אינם ריקות
                    if (agent._Location != null&&target._Location!=null)
                    {// שליחת מיקום הסוכן שהתקבל + המטרה לפונקציה לחישוב מרחק
                        Double distance = Handler.GetDistance(target._Location, agent._Location);
                        if (distance < 200)
                        {
                            //בדיקה האם יש משימה קיימת עבור הסוכן והמטרה
                            Mission existMission = await _context.missions.Include(x => x._agent).Include(x => x._target).Where(x => x._agent.id == agent.id && x._target.id == target.id).FirstOrDefaultAsync();
                            if (existMission == null)
                            {
                                // יצירת המשימה + עדכון הסוכן והמטרה המקושרים
                                Mission mission = new Mission();
                                mission._agent = agent;
                                mission._target = target;
                                // עדכון סטטוס המשימה להצעה
                                mission.Status = MissionStatus.Suggestion;
                                await _context.missions.AddAsync(mission);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }
            }
        }
    }
}


































