using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mossad_API.DAL;
using Mossad_API.Moddels.API_Moddels;
using Mossad_API.Moddels.DBModdels;
using Mossad_API.Moddels.Enums;
using Mossad_API.Services;
using System;

namespace Mossad_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MVCController : ControllerBase
    {
        private readonly MossadAPIDbContext _context;

        public MVCController(MossadAPIDbContext context)
        {
            _context = context;
        }




        // פונקציה להחזרת כל הנתונים עבור הMVC
        [HttpGet("GetAllData")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllData()
        {
            // שליפה של טבלת המשימות, הסוכנים, והמטרות
            List<Mission> missions = await _context.missions.ToListAsync();

            List<Agent> agnets = await _context.agents.ToListAsync();

            List<Target> targets = await _context.targets.ToListAsync();

            // יצירת מופע עבור המודל שאותו נשלח  
            GeneralDataReponse response = new GeneralDataReponse();
            // אכלוס הנתונים
            response.AgentsCount = agnets.Count;
            response.AgentsInActivityCount = agnets.Count(x => x.Status == AgentStatus.InActivity);
            response.TargetsCount = targets.Count;
            response.TargetsKilledCount = targets.Count(x => x.Status == TargetStatus.Eliminated);
            response.MissionsCount = missions.Count;
            response.MissionsAssignedCount = missions.Count(x => x.Status == MissionStatus.Assigned);
            // חישוב יחס של הסוכנים למטרות מבוצע על-יד חלוקה של כמות הסוכנים בכמות המטרות
            response.RelationOfAgentsToTargets = response.AgentsCount / response.TargetsCount;
            // בדיקה אם ריימות מטרות שלא בסטטוס חיסול
            if ((response.TargetsCount - response.TargetsKilledCount) > 0)
            {
                // מתבצע עדכון של זמינות סוכנים למטרות על ידי חלוקה של כמות הסוכנים פחות הסוכנים הפעילים לחלק למטרות פחות המטרות שבסטטוס חוסל
                response.RelationOfAvaliableAgentsToTargets = (response.AgentsCount - response.AgentsInActivityCount) / (response.TargetsCount - response.TargetsKilledCount);
            }
            // שליחת הליסט 
            return StatusCode(
              StatusCodes.Status200OK,
              response
              );
        }



        // פונקציב לקבלת נתוני הסוכנים עבור הMVC
        [HttpGet("GetAgentsData")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAgentsData()
        {
            // יצירת ליסט מסוג המודל שאותו נשלח
            List<GetAgentResponse> agnetsData = new List<GetAgentResponse>();
            // שליפה של הסוכנים + המיקום
            List<Agent> agnets = await _context.agents.Include(x => x._Location).ToListAsync();
            foreach (Agent agent in agnets)
            {
                // עבור כל איטרציה בלולאה אני מייצר מופע של המודל שנשלח
                GetAgentResponse agentResponse = new GetAgentResponse();
                // עדכון הנתונים
                agentResponse.AgentX = agent._Location.X;
                agentResponse.AgentY = agent._Location.Y;
                agentResponse.Status = agent.Status;
                agentResponse.NickName = agent.nickname;
                agentResponse.PhotoUrl = agent.photo_url;

                // שליפת המשימות עבור הסוכן שעליו יש איטרציה ובסטוס הסתיים
                List<Mission> agentMissions = await _context.missions.Where(x => x._agent.id == agent.id && x.Status == MissionStatus.Ended).ToListAsync();
                // בדיקה האם חזר Null ואם כן אני מעדכן את את כמות החיסולים
                if (agentMissions != null)
                    agentResponse.KillsCount = agentMissions.Count;
                // בדיקה אם הסטטוס של הסוכן הוא פעיל
                if (agent.Status == AgentStatus.InActivity)
                {
                    // שליפה של הסוכן המקושר למשימה כשהוא בסטטוס בהצעה
                    Mission mission = await _context.missions.FirstOrDefaultAsync(x => x._agent.id == agent.id && x.Status == MissionStatus.Assigned);
                    if (mission != null)
                    {
                        // עדכון הID והזמן נותר
                        agentResponse.MissionId = mission.Id;
                        agentResponse.TimeLeft = mission.TimeLeft;
                    }
                }
                // בסיום כל איטרציה מוסיף למשתנה
                agnetsData.Add(agentResponse);
            }
            // שליחת הליסט 
            return StatusCode(
              StatusCodes.Status200OK,
              agnetsData
              );
        }



        // פונקציה לשליחת נתונ המטרות לMVC
        [HttpGet("GetTargetsData")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTargetsData()
        {
            // יצירת מופע מסוג המודל שאותו נשלח
            List<GetTargetResponse> getTargets = await _context.targets.Include(x => x._Location).Select(
            x => new GetTargetResponse()
            // יצירת אובייקט מסוג המודל ואכלוס הנתונים בו
            {
                Id = x.id,
                Name = x.name,
                Position = x.position,
                PhotoUrl = x.photo_url,
                TargetX = x._Location.X,
                TargetY = x._Location.Y,
                Status = x.Status
            }).ToListAsync();   
                // שליחת הליסט 
                return StatusCode(
                  StatusCodes.Status200OK,
                  getTargets
              );
        }

    }
}


