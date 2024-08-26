using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mossad_API.DAL;
using Mossad_API.Moddels;
using Mossad_API.Moddels.API_Moddels;
using Mossad_API.Moddels.DBModdels;
using Mossad_API.Moddels.Enums;
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
        
        //  פונקציה לקבלת משימות בסטטוס הצעה
        // הפונקציה  מחזירה לMVC
        [HttpGet("GetSuggestionMissions")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSuggestionMissions()
        {
            // יצירת ליסט שמכיל מודל של הנתונים שיופיעו בתצוגה 
            // מתבצעת שליפה של המשימות בסטטוס הצעה בצירוף הסוכנים והמטרה המקושרים ויצירת אובייקט חדש שמכיל את הנתונים הנדרשים לתצוגה
            List<GetMissionResponse> missions = await _context.missions.Include(x => x._agent).ThenInclude(x => x._Location).Include(x => x._target).ThenInclude(x => x._Location).Where(x => x.Status == MissionStatus.Suggestion).Select(
                x => new GetMissionResponse()
                {
                    Id = x.Id,
                    AgentName = x._agent.nickname,
                    AgentX = x._agent._Location.X,
                    AgentY = x._agent._Location.Y,
                    TargetName = x._target.name,
                    TargetX = x._target._Location.X,
                    TaregtY = x._target._Location.Y,
                    Distance = Handler.GetDistance(x._target._Location, x._agent._Location),
                    TimeLeft = x.TimeLeft
                }).ToListAsync();
            // שליחת הליסט 
            return StatusCode(
              StatusCodes.Status200OK,
              new { missions }
              );
        }




        // עדכון סטטוס המשימה
        [HttpPut("UpdateMissionStatus/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> UpdateMissionStatus(int id, UpdateMissionRequest updateMissionRequest)
        {
            try
            {
                // שליפת המשימה + הסוכן והמטרה המקושרים לפי הId שהתקבל
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
                // בדיקה אם המשימה הסתיימה
                if (mission.Status == MissionStatus.Ended)
                {
                    return StatusCode(400, new { error = "The task has ended and therefore cannot be updated" });
                }
                // קריאה לפונקציה לחישוב המרחק בין הסוכן למטרה
                Double distance = Handler.GetDistance(mission._target._Location, mission._agent._Location);

                if (distance > 200)
                {
                    _context.missions.Remove(mission);
                    await _context.SaveChangesAsync();

                    return StatusCode(400, new { error = "Can't be assigned to the target because of the distance" });
                }
                // עדכון סטטוס המשימה למצוות  
                mission.Status = MissionStatus.Assigned;
                // חישוב הזמן הנותר לביצוע על-ידי חלוקה ב5
                mission.TimeLeft = distance / 5;
                // עדכון הסוכן לסטטוס בפעילות
                mission._agent.Status = AgentStatus.InActivity;
                // עדכון השינויםם בדאטה + שמירה
                _context.missions.Update(mission);
                await _context.SaveChangesAsync();
                // שליחת הסוכן והמטרה לפונקציה עבור מחיקה למשימות אחרות המקושרות לסוכן והמטרה
                await DeleteMissions(mission._agent.id, mission._target.id);

                return Ok();
            }
            catch
            {
                return BadRequest();
            }

        }




        // עדכון המשימה
        [HttpPost("update")]
        [Produces("application/json")]

        public async Task UpdateMissions()
        {
            // שליפה מהדאטה של המשימות + הסוכן והמטרה המקושרים כשהסטטוס של המשימה הוא בציוות
            List<Mission> missions = await _context.missions.Include(x => x._target).Include(x => x._agent).Where(x => x.Status == MissionStatus.Assigned).ToListAsync();

            foreach (Mission mission in missions)
            {// עבור כל משימה מתבצע שליפה של המטרה והסוכן + המיקומים לפי הId 

                Target target = await _context.targets.Include(x => x._Location).FirstOrDefaultAsync(x => x.id == mission._target.id);

                Agent agent = await _context.agents.Include(x => x._Location).FirstOrDefaultAsync(x => x.id == mission._agent.id);

                // קריאה לפונקציה לחישוב המרחק בין הסוכן למטרה במשימה המקושרת
                Double distance = Handler.GetDistance(mission._target._Location, mission._agent._Location);


                if (distance > 200)
                {
                    _context.missions.Remove(mission);
                    await _context.SaveChangesAsync();

                }
                // שליחת הסוכן והמטרה לפונקציה שתחשב את היחס במבחינת הכיון ביניהם
                string direction = Handler.CalculateDirection(mission._agent, mission._target);
                // עדכון המיקום על-ידי שליחת הסוכן + הכיוון שעליו לנוע
                Location newLocation = Handler.CalculateLocation(mission._agent._Location, direction);

                // עדכון הסוכן בדאטה
                _context.agents.Update(agent);
                // שינוי המיקום ע"פ המיקום החדש שמגיע
                agent._Location = newLocation;

                // חישוב המרחק החדש על-ידי שליחה של המיקום החדש של הסוכן + המיקום של המטרה
                Double newDistance = Handler.GetDistance(newLocation, mission._target._Location);
                // חישוב הזמן הנותר על-ידי חלוקה ב5
                Double timeLeft = newDistance / 5;

                // אם המרחק החדש הוא 0 - דהיינו הם על אותו מקןם מתבצעת קריאה לםונקציה שמחסלת את המטרה
                if (newDistance == 0)
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




        // פונקציה למחיקת משימה
        private async Task DeleteMissions(int agendId, int targetId)
        {
            // שליפה מהדאטה מטבלת המשימות של כל המשימות המקושרות לסוכן לפי
            // הid שהתקבל
            // וכשהסטטוס של המשימה לא בציוות
            List<Mission> deleteMissionsForAgent = await _context.missions.Include(x => x._agent).Where(x => x._agent.id == agendId && x.Status != MissionStatus.Assigned).ToListAsync();

            // שליפה מהדאטה מטבלת המשימות של כל המשימות המקושרות למטרה לפי
            // הid שהתקבל
            // וכשהסטטוס של המשימה לא בציוות
            List<Mission> deleteMissionsForTarget = await _context.missions.Include(x => x._target).Where(x => x._target.id == agendId && x.Status != MissionStatus.Assigned).ToListAsync();

            // מחיקת המשימות עבור הסוכן
            _context.missions.RemoveRange(deleteMissionsForAgent);
            // מחיקת המשימות עבור המטרה
            _context.missions.RemoveRange(deleteMissionsForTarget);


            await _context.SaveChangesAsync();
        }





        // פונקציה לחיסול המטרה
        private async void KillTarget(Mission mission)
        {
            // עדכון סטטוס המטרה לחוסל,סטטוס הסוכן לרדום, וסטטוס המשימה להסתיימה
            mission._target.Status = TargetStatus.Eliminated;
            mission._agent.Status = AgentStatus.Dormant;
            mission.Status = MissionStatus.Ended;
            // עדכון ושמירת הנתונים
            _context?.missions.Update(mission);

            await _context.SaveChangesAsync();
        }
    }
}




