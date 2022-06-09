namespace KanbanBoard.Controllers;

[Route("/api")]
[ApiController]
public class ProjectsController : ControllerBase
{
    [HttpGet("[controller]"), Authorize]
    public async Task<IActionResult> GetProjects([FromServices] ApplicationContext context, [FromServices] RequestData requestData)
    {
        return Ok(
             await context.ProjectMembers
                .Where(p => p.UserId == requestData.UserId)
                .Select(p => p.Project)
                .OrderBy(p => p.Id)
                .ProjectToType<ProjectGetDto>()
                .ToListAsync()
            );
    }
    
    [HttpGet("[controller]/{id:int}")]
    public async Task<IActionResult> GetProject(int id,
        [FromServices] ApplicationContext context,
        [FromServices] RequestData requestData)
    {
        if (!context.ProjectMembers.Any(p => p.UserId == requestData.UserId && p.ProjectId == id))
            return Problem(statusCode: 403);
        
        var project = await context.Projects.Where(t => t.Id == id).ProjectToType<ProjectGetDto>().FirstOrDefaultAsync();
        return project is {} ? Ok(project) : Problem(title: "Project not found", statusCode: 404);
    }
    
    [HttpPost("[controller]")]
    public async Task<IActionResult> AddProject(
        [FromServices] ApplicationContext context,
        [FromServices] RequestData requestData,
        [FromBody] ProjectPostDto newProject)
    {
        Project project = newProject.Adapt<Project>();
        project.UserId = requestData.UserId!.Value;
        context.Projects.Add(project);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project.Adapt<ProjectGetDto>());
    }
    
    [HttpDelete("[controller]/{id:int}"), ProjectAccessActionFilter]
    public async Task<IActionResult> DeleteProject(int id,
        [FromServices] ApplicationContext context)
    {
        int c = await context.Projects.Where(p => p.Id == id).BatchDeleteAsync();
        return c > 0 ? StatusCode(204) : Problem(title: "Project not found", statusCode: 404);
    }
    
    [HttpPatch("[controller]/{id:int}"), ProjectAccessActionFilter]
    public async Task<IActionResult> PathProject(int id,
        [FromServices] ApplicationContext context,
        [FromBody] ProjectPatchDto projectPatch,
        [FromServices] RequestData requestData)
    {
        int c = await context.Projects
            .Where(t => t.Id == id)
            .BatchUpdateAsync(projectPatch.Adapt<Project>(), projectPatch.ChangedProperties.ToList());
        return c > 0 ? StatusCode(204) : Problem(title: "Project not found", statusCode: 404);
    }
    
    [HttpGet("[controller]/{id:int}/users"), ProjectAccessActionFilter]
    public async Task<IActionResult> GetProjectUsers(int id,
        [FromServices] ApplicationContext context,
        [FromServices] RequestData requestData)
    {
        var users = await context.ProjectMembers
            .Where(p => p.ProjectId == id)
            .Select(p =>  new { p.User.Id, p.User.Email, p.User.Name })
            .ToListAsync();
        if (users.All(u => u.Id != requestData.UserId!.Value)) return Problem(statusCode: 403);
        return users.Count > 0 ? Ok(users) : Problem(title: "Project not found", statusCode: 404);
    }
    
    [HttpPost("[controller]/{id:int}/users")]
    public async Task<IActionResult> AddProjectMember(int id,
        [FromServices] ApplicationContext context,
        [FromServices] RequestData requestData,
        [FromBody] ProjectMemberPostDto newMember)
    {
        int? projectOwnerId = await context.Projects.Where(p => p.Id == id).Select(u => (int?)u.UserId).FirstOrDefaultAsync();
        
        if (projectOwnerId == null) 
            return Problem(title: "Project not found", statusCode: 404);
        
        if (projectOwnerId != requestData.UserId)
            return Problem(title: "You must be the owner of the project", statusCode: 403);
        
        var user = await context.Users
            .Where(u => u.Email == newMember.Email)
            .Select(u => new { u.Id, u.Email, u.Name })
            .FirstOrDefaultAsync();
        
        if (user == null) 
            return Problem(title: "User not found", statusCode: 404);
        
        if (await context.ProjectMembers.AnyAsync(m => m.ProjectId == id && m.UserId == user.Id))
            return Problem(title: "The user is already a member of the project", statusCode: 404);

        ProjectMember member = new() { ProjectId = id, UserId = user.Id };
        context.ProjectMembers.Add(member);
        await context.SaveChangesAsync();
        
        return Ok(user);
    }
    
    [HttpDelete("[controller]/{projectId:int}/users/{userId:int}")]
    public async Task<IActionResult> DeleteProjectMember(int projectId, int userId,
        [FromServices] ApplicationContext context,
        [FromServices] RequestData requestData)
    {
        int? projectOwnerId = await context.Projects.Where(p => p.Id == projectId).Select(u => (int?)u.UserId).FirstOrDefaultAsync();
        
        if (projectOwnerId == null) 
            return Problem(title: "Project not found", statusCode: 404);
        
        if (projectOwnerId != requestData.UserId)
            return Problem(title: "You must be the owner of the project", statusCode: 403);
        
        if (projectOwnerId == userId)
            return Problem(title: "You can't delete yourself", statusCode: 404);

        int c = await context.ProjectMembers.Where(m => m.ProjectId == projectId && m.UserId == userId).BatchDeleteAsync();
        
        return c > 0 ? StatusCode(204) : Problem(title: "Member not found", statusCode: 404);
    }
}