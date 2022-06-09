using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace KanbanBoard.Controllers;

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
