using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mossad_API.DAL;
using Mossad_API.Moddels;
using Mossad_API.Moddels.DBModdels;
using Mossad_API.Moddels.Enums;
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
                    // עדכון הסוכן בפרמטקרים הבאים מהסימולציה
                    Agent agent = new Agent();
                    agent.nickname = createAgentRequest.nickname;
                    agent.photo_url = createAgentRequest.photoUrl;  
                    agent.Status = AgentStatus.Dormant;
                    // הוספת הנתונים ושמירתם 
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
                // שליפה של  סוכן המבוקש + המיקום 
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
                // יצירת מופע למיקום + עדכון הנתונים 
                agent._Location = new Location();
                agent._Location.X = updateLocationRequest.x;
                agent._Location.Y = updateLocationRequest.y;
                // עדכון הנתונים בDB ושמירתם
                _context.Update(agent);
                await _context.SaveChangesAsync();
                // שליחת הסוכן לפונקציה שמייצרת משימה עבורן
                await CreateMissionsForAgent(agent);

                return Ok();
            }
            catch
            {
                return StatusCode(500, new { error = "Connection failed" });
            }
        }

             
        
        // פונקציה להזזת סוכן
        [HttpPut("{id}/move")]
        [Produces("application/json")]
        public async Task<IActionResult> MoveAgent(int id, MoveRequest moveRequest)
        {
            try
            {
                // שליפה מהדאטה-בייס של הסוכן + המיקום
                // לפי הId שהתקבל
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
                // אם הסוכן בפעילות לא ניתן להזיז
                if (agent.Status == AgentStatus.InActivity)
                {
                    return StatusCode(400, new { error = "Agent cannot be moved  in action" });
                }
                // קריאה לפונקציה שמזיזה את הסוכן לפי מיקומו והכיוון שהתקבל 
                Location newLocation = Handler.CalculateLocation(agent._Location, moveRequest.direction);
                // אם המיקום החדש מחוץ לגבולות המטריצה חוזרת שגיאה מתאימה + המיקום 
                if (newLocation.X > 1000 || newLocation.Y > 1000 || newLocation.X < 0 || newLocation.Y < 0)
                {
                    return StatusCode(
                400,
                new
                {
                    error = "Agent cannot be moved  outside to the borders the matrix.",
                    location = agent._Location
                }); 
                }
                // עדכון המיקום החדש + עדכון בדאטה + שמירה
                agent._Location = newLocation;
                _context.Update(agent);
                await _context.SaveChangesAsync();

                 await CreateMissionsForAgent(agent);

                return Ok();
            }
            catch
            {
                return StatusCode(500, new { error = "Connection failed" });
            }
        }

        // פונקציה ליצירת משימה עבור סוכן
        private async Task CreateMissionsForAgent(Agent agent)
        {
            // "שליפה מבסיס הנתונים לתוך ליסט את כל המטרות + המיקומים שלהם כשהסטטוס הוא "חי
            List<Target> targets = await _context.targets.Include(x => x._Location).Where(x => x.Status == TargetStatus.Live).ToListAsync();
           
            if (targets != null && targets.Count > 0)
            {
                foreach (Target target in targets)
                {// בדיקה אם המיקומים של הסוכן והמטרה אינם ריקות
                    if(target._Location != null&& agent._Location!=null)
                    { // שליחת מיקום הסוכן שהתקבל + המטרה לפונקציה לחישוב מרחק
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
                 









     








