using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Task = KanbanBoard.Models.Task;

namespace KanbanBoard.Controllers;

[Route("/api"), Authorize]
[ApiController]
public class TasksController : ControllerBase
{
    [HttpGet("projects/{id:int}/[controller]"), ProjectAccessActionFilter]
    public async Task<IActionResult> GetProjectTasks([FromServices] ApplicationContext context, int id)
    {
        if (!await context.Projects.AnyAsync(p => p.Id == id)) return Problem(title: "Project not found", statusCode: 404);
        
        var tasks = await context.Tasks
            .OrderByDescending(p => p.Id)
            .Where(t => t.ProjectId == id)
            .OrderBy(t => t.Id)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Text,
                t.CreationDate,
                t.UserId,
                t.Status
            })
            .ToListAsync();

        var users = await context.Users
            .Where(u => tasks.Select(t => t.UserId).Distinct().Contains(u.Id))
            .Select(u => new { u.Id, u.Name })
            .ToListAsync();
        
        return Ok(new
        {
            Tasks = tasks
                .GroupBy(t => t.Status)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(t => new { t.Id, t.Name, t.Text, t.CreationDate, t.UserId })),
            Users = users
        });
    }

    [HttpGet("[controller]/{id:int}"), TaskAccessActionFilter]
    public async Task<IActionResult> GetTask([FromServices] ApplicationContext context, int id)
    {
        var task = await context.Tasks.Where(t => t.Id == id).ProjectToType<TaskGetDto>().FirstOrDefaultAsync();
        return task is {} ? Ok(task) : Problem(title: "Task not found", statusCode: 404);
    }

    [HttpPost("projects/{id:int}/[controller]"), ProjectAccessActionFilter]
    public async Task<IActionResult> AddTask(int id, [FromServices] ApplicationContext context, [FromServices] RequestData requestData,
        [FromBody] TaskPostDto newTask)
    {
        Task task = newTask.Adapt<Task>();
        task.UserId = requestData.UserId!.Value;
        task.ProjectId = id;
        task.CreationDate = DateTime.Now;
        context.Tasks.Add(task);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task.Adapt<TaskGetDto>());
    }
    
    [HttpDelete("[controller]/{id:int}"), TaskAccessActionFilter]
    public async Task<IActionResult> DeleteTask([FromServices] ApplicationContext context, int id)
    {
        int c = await context.Tasks.Where(t => t.Id == id).BatchDeleteAsync();
        return c > 0 ? StatusCode(204) : Problem(title: "Task not found", statusCode: 404);
    }
    
    [HttpPatch("[controller]/{id:int}"), TaskAccessActionFilter]
    public async Task<IActionResult> PathTask(
        [FromServices] ApplicationContext context, int id,
        [FromBody] TaskPatchDto taskPatch)
    {
        int c = await context.Tasks
            .Where(t => t.Id == id)
            .BatchUpdateAsync(taskPatch.Adapt<Task>(), taskPatch.ChangedProperties.ToList());
        return c > 0 ? StatusCode(204) : Problem(title: "Task not found", statusCode: 404);
    }
}

public class TaskAccessActionFilterAttribute : ActionFilterAttribute
{
    public override async System.Threading.Tasks.Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        int taskId = (int)context.ActionArguments["id"]!;
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationContext>();
        var requestData = context.HttpContext.RequestServices.GetRequiredService<RequestData>();
        var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

        if (!await dbContext.ProjectMembers.AnyAsync(p => p.UserId == requestData.UserId && p.ProjectId == 
                dbContext.Tasks.Where(t => t.Id == taskId).Select(t => t.ProjectId).Single()))
        {
            ProblemDetails problem = problemDetailsFactory.CreateProblemDetails(context.HttpContext, statusCode: 403);
            context.Result = new ObjectResult(problem) { StatusCode = problem.Status };
        } else
            await next();
    }
}

public class ProjectAccessActionFilterAttribute : ActionFilterAttribute
{
    public override async System.Threading.Tasks.Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        int projectId = (int)context.ActionArguments["id"]!;
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationContext>();
        var requestData = context.HttpContext.RequestServices.GetRequiredService<RequestData>();
        var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

        if (!await dbContext.ProjectMembers.AnyAsync(p => p.UserId == requestData.UserId && p.ProjectId == projectId))
        {
            ProblemDetails problem = problemDetailsFactory.CreateProblemDetails(context.HttpContext, statusCode: 403);
            context.Result = new ObjectResult(problem) { StatusCode = problem.Status };
        } else
            await next();
    }
}
