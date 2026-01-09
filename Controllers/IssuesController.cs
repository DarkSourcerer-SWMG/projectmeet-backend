using Microsoft.AspNetCore.Mvc;
using ProjectMeet.Data;
using ProjectMeet.Models;
using ProjectMeet.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ProjectMeet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IssuesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IssuesController(AppDbContext context)
        {
            _context = context;
        }

        // UCZEŃ zgłasza problem
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateIssue(IssueDto dto)
        {
            var issue = new Issue
            {
                UserId = dto.UserId,
                Title = dto.Title,
                Description = dto.Description,
                RepoLink = dto.RepoLink
            };

            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();

            return Ok("Issue submitted");
        }

        // ADMIN podgląda problemy
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetIssues()
        {
            return Ok(await _context.Issues.Include(u => u.User).ToListAsync());
        }
    }
}
