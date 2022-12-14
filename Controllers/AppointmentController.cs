using AppointmentApi.DTO;
using AppointmentApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppointmentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly AppointmentApiContext _context;

        private async Task<bool> CheckConflictAsync(DateTime date, int userId, int id = 0)
        {
            return await _context.Calendars.Where(s => s.Date == date 
            && s.UserId == userId
            && s.CalendarId != id).OrderBy(s => s.Date).AnyAsync();
        }

        private int GetUserId()
        {
            var identity = User.Claims.ToList();
            var userId = identity.FirstOrDefault(s => s.Type == "Id").Value;
            return Int32.Parse(userId);
        }

        public AppointmentController(AppointmentApiContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CalendarDTO>> GetCalendars(int id)
        {
            int userId = GetUserId();

            var calendar = await _context.Calendars.FirstOrDefaultAsync(s => s.CalendarId == id && s.UserId == userId);

            if (calendar == null)
            {
                return NotFound();
            }

            return new CalendarDTO()
            {
                Date = calendar.Date,
                Title = calendar.Title,
                Notes = calendar.Notes
            };
        }

        [HttpPost]
        public async Task<ActionResult<Calendars>> PostCalendarAsync([FromBody] CalendarDTO payload)
        {
            if (payload == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(payload.Title))
            {
                ModelState.AddModelError("title", "Title is required");
                return BadRequest(ModelState);
            }

            if (payload.Date < System.DateTime.Now)
            {
                ModelState.AddModelError("date", "Invalid date");
                return BadRequest(ModelState);
            }

            if (await CheckConflictAsync(payload.Date, GetUserId()))
            {
                ModelState.AddModelError("date", "Conflict calendar");
                return BadRequest(ModelState);
            }

            var model = new Calendars()
            {
                UserId = GetUserId(),
                Title = payload.Title,
                Date = payload.Date,
                Notes = payload.Notes,
            };

            _context.Calendars.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCalendars", new { id = model.CalendarId }, model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCalendarAsync(int id, [FromBody] CalendarDTO payload)
        {
            if (payload == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(payload.Title))
            {
                ModelState.AddModelError("title", "Title is required");
                return BadRequest(ModelState);
            }

            if (payload.Date < System.DateTime.Now)
            {
                ModelState.AddModelError("date", "Invalid date");
                return BadRequest(ModelState);
            }

            if (await CheckConflictAsync(payload.Date, id))
            {
                ModelState.AddModelError("date", "Conflict calendar");
                return BadRequest(ModelState);
            }

            int userId = GetUserId();

            var calendar = await _context.Calendars.FirstOrDefaultAsync(s => s.CalendarId == id && s.UserId == userId);

            if (calendar == null)
            {
                return NotFound();
            }

            calendar.Date = payload.Date;
            calendar.Notes = payload.Notes;
            calendar.Title = payload.Title;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Calendars>>> ListCalendarsAsync(DateTime? from, DateTime? to, int? skip, int? take)
        {
            var userId = GetUserId();
            var calendars = _context.Calendars.Where(s => s.UserId == userId).AsQueryable();

            if (from.HasValue)
            {
                calendars = calendars.Where(s => s.Date >= from);
            }

            if (to.HasValue)
            {
                calendars = calendars.Where(s => s.Date <= to);
            }

            if (skip.HasValue)
            {
                calendars = calendars.Skip((int)skip);
            }

            if (take.HasValue)
            {
                calendars = calendars.Take((int)take);
            }

            return await calendars.OrderBy(s => s.Date).ToListAsync();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<CalendarDTO>> DeleteProductAsync(int id)
        {
            var userId = GetUserId();
            var calendar = await _context.Calendars.FirstOrDefaultAsync(s => s.CalendarId == id && s.UserId == userId);

            if (calendar == null)
            {
                return NotFound();
            }

            _context.Calendars.Remove(calendar);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("conflict")]
        public async Task<ActionResult> IsConflictAsync(DateTime date, int userId)
        {
            var conflictedCalendars = await _context.Calendars.Where(s => s.Date == date && s.UserId == userId).OrderBy(s => s.Date).ToListAsync();

            return new OkObjectResult(new
            {
                conflict = conflictedCalendars.Any(),
                conflictedCalendars
            });
        }
    }
}
