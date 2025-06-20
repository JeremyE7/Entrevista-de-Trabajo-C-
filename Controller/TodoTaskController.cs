
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TodoApi.Data;
using TodoApi.DTOs;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TodoTasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TodoTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoTaskDto>>> GetTodoTasks()
        {
            var userId = GetCurrentUserId();
            var tasks = await _context.TodoTasks
                .Where(t => t.UserId == userId)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Replies)
                .Select(t => new TodoTaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    CreatedAt = t.CreatedAt,
                    Comments = t.Comments.Where(c => c.ParentCommentId == null)
                        .Select(c => MapCommentToDto(c)).ToList()
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoTaskDto>> GetTodoTask(int id)
        {
            var userId = GetCurrentUserId();
            var task = await _context.TodoTasks
                .Where(t => t.Id == id && t.UserId == userId)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Replies)
                .FirstOrDefaultAsync();

            if (task == null)
            {
                return NotFound();
            }

            var taskDto = new TodoTaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                Comments = task.Comments.Where(c => c.ParentCommentId == null)
                    .Select(c => MapCommentToDto(c)).ToList()
            };

            return Ok(taskDto);
        }

        [HttpPost]
        public async Task<ActionResult<TodoTaskDto>> PostTodoTask(CreateTodoTaskDto createDto)
        {
            var userId = GetCurrentUserId();
            var task = new TodoTask
            {
                Title = createDto.Title,
                Description = createDto.Description,
                UserId = userId
            };

            _context.TodoTasks.Add(task);
            await _context.SaveChangesAsync();

            var taskDto = new TodoTaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                Comments = new List<CommentDto>()
            };

            return CreatedAtAction(nameof(GetTodoTask), new { id = task.Id }, taskDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoTask(int id, UpdateTodoTaskDto updateDto)
        {
            var userId = GetCurrentUserId();
            var task = await _context.TodoTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                return NotFound();
            }

            task.Title = updateDto.Title;
            task.Description = updateDto.Description;
            task.IsCompleted = updateDto.IsCompleted;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoTaskExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoTask(int id)
        {
            var userId = GetCurrentUserId();
            var task = await _context.TodoTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                return NotFound();
            }

            _context.TodoTasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TodoTaskExists(int id)
        {
            return _context.TodoTasks.Any(e => e.Id == id);
        }

        private static CommentDto MapCommentToDto(Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                CommentText = comment.CommentText,
                IsUpdated = comment.IsUpdated,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                ParentCommentId = comment.ParentCommentId,
                Replies = comment.Replies.Select(r => MapCommentToDto(r)).ToList()
            };
        }
    }
}
