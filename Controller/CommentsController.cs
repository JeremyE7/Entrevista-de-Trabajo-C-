
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
    [Route("api/todotasks/{taskId}/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(int taskId)
        {
            var userId = GetCurrentUserId();
            var taskExists = await _context.TodoTasks
                .AnyAsync(t => t.Id == taskId && t.UserId == userId);

            if (!taskExists)
            {
                return NotFound("Task not found");
            }

            var comments = await _context.Comments
                .Where(c => c.TaskId == taskId && c.ParentCommentId == null)
                .Include(c => c.Replies)
                .ToListAsync();

            var commentDtos = comments.Select(c => MapCommentToDto(c)).ToList();
            return Ok(commentDtos);
        }

        [HttpPost]
        public async Task<ActionResult<CommentDto>> PostComment(int taskId, CreateCommentDto createDto)
        {
            var userId = GetCurrentUserId();
            var taskExists = await _context.TodoTasks
                .AnyAsync(t => t.Id == taskId && t.UserId == userId);

            if (!taskExists)
            {
                return NotFound("Task not found");
            }

            // Validate parent comment if provided
            if (createDto.ParentCommentId.HasValue)
            {
                var parentExists = await _context.Comments
                    .AnyAsync(c => c.Id == createDto.ParentCommentId.Value && c.TaskId == taskId);
                
                if (!parentExists)
                {
                    return BadRequest("Parent comment not found");
                }
            }

            var comment = new Comment
            {
                CommentText = createDto.CommentText,
                TaskId = taskId,
                ParentCommentId = createDto.ParentCommentId,
                UserId = userId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var commentDto = new CommentDto
            {
                Id = comment.Id,
                CommentText = comment.CommentText,
                IsUpdated = comment.IsUpdated,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                ParentCommentId = comment.ParentCommentId,
                Replies = new List<CommentDto>()
            };

            return CreatedAtAction(nameof(GetComments), new { taskId }, commentDto);
        }

        [HttpPut("{commentId}")]
        public async Task<IActionResult> PutComment(int taskId, int commentId, UpdateCommentDto updateDto)
        {
            var userId = GetCurrentUserId();
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.TaskId == taskId && c.UserId == userId);

            if (comment == null)
            {
                return NotFound();
            }

            comment.CommentText = updateDto.CommentText;
            comment.IsUpdated = true;
            comment.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(commentId))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int taskId, int commentId)
        {
            var userId = GetCurrentUserId();
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.TaskId == taskId && c.UserId == userId);

            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
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
