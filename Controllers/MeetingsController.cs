    // ...existing code...
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectMeet.Data;
using ProjectMeet.Models;
using ProjectMeet.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ProjectMeet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MeetingsController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeeting(int id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null)
                return NotFound();

            _context.Meetings.Remove(meeting);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        private readonly AppDbContext _context;

        public MeetingsController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMeetings()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid token");
            }

            var meetings = await _context.UserMeetings
                .Where(um => um.UserId == userId)
                .Include(um => um.Meeting)
                .Select(um => um.Meeting)
                .ToListAsync();

            return Ok(meetings);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateMeeting(Meeting meeting)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid token");
            }

            meeting.Date = DateTime.SpecifyKind(meeting.Date, DateTimeKind.Utc);
            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            // Automatically add creator as member of the meeting
            var userMeeting = new UserMeeting
            {
                UserId = userId,
                MeetingId = meeting.Id,
                SignUpDate = DateTime.UtcNow
            };
            _context.UserMeetings.Add(userMeeting);
            await _context.SaveChangesAsync();

            Console.WriteLine($"DATE: {meeting.Date}, KIND: {meeting.Date.Kind}");
            return Ok(meeting);
        }
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(SignUpDto dto)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "SELECT public.signup_for_meeting(CAST({0} AS INT), CAST({1} AS INT))",
                    dto.UserId, dto.MeetingId);

                return Ok("User signed up successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.Name, u.Email })
                .ToListAsync();

            return Ok(users);
        }

        [Authorize]
        [HttpPost("addParticipant")]
        public async Task<IActionResult> AddParticipant(AddParticipantDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest("Email is required");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                    return NotFound("User not found");

                await _context.Database.ExecuteSqlRawAsync(
                    "SELECT public.signup_for_meeting(CAST({0} AS INT), CAST({1} AS INT))",
                    user.Id, dto.MeetingId);

                return Ok(new { message = "Participant added" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
