using Task = KanbanBoard.Models.Task;

namespace KanbanBoard.Controllers;

[Route("/api")]
[ApiController]
public class CommentsController : ControllerBase
{
    [HttpGet("tasks/{id:int}/[controller]")]
    public async Task<IActionResult> GetCommentComments([FromServices] ApplicationContext context, int id)
    {
        if (!await context.Tasks.AnyAsync(p => p.Id == id)) return Problem(title: "Task not found", statusCode: 404);
        
        var comments = await context.Comments
            .OrderByDescending(c => c.Id)
            .Where(c => c.TaskId == id)
            .OrderBy(c => c.Id)
            .ProjectToType<CommentGetDto>()
            .ToListAsync();

        var users = await context.Users
            .Where(u => comments.Select(c => c.UserId).Distinct().Contains(u.Id))
            .Select(u => new { u.Id, u.Name })
            .ToListAsync();
        
        return Ok(new
        {
            Comments = comments,
            Users = users
        });
    }

    [HttpGet("[controller]/{id:int}")]
    public async Task<IActionResult> GetComment([FromServices] ApplicationContext context, int id)
    {
        var comment = await context.Comments.Where(t => t.Id == id).ProjectToType<CommentGetDto>().FirstOrDefaultAsync();
        return comment is {} ? Ok(comment) : Problem(title: "Comment not found", statusCode: 404);
    }

    [HttpPost("[controller]")]
    public async Task<IActionResult> AddComment([FromServices] ApplicationContext context, [FromServices] RequestData requestData,
        [FromBody] CommentPostDto newComment)
    {
        Comment comment = newComment.Adapt<Comment>();
        comment.UserId = requestData.UserId!.Value;
        comment.CreationDate = DateTime.Now;
        context.Comments.Add(comment);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment.Adapt<CommentGetDto>());
    }
}
