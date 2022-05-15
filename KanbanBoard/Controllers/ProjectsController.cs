namespace KanbanBoard.Controllers;

[Route("/api")]
[ApiController]
public class ProjectsController : ControllerBase
{
    [HttpGet("[controller]")]
    public async Task<IActionResult> GetProjects([FromServices] ApplicationContext context)
    {
        return Ok(await context.Projects.OrderBy(p => p.Id).ProjectToType<ProjectGetDto>().ToListAsync());
    }
    
    [HttpGet("[controller]/{id:int}")]
    public async Task<IActionResult> GetProject([FromServices] ApplicationContext context, int id)
    {
        var project = await context.Projects.Where(t => t.Id == id).ProjectToType<ProjectGetDto>().FirstOrDefaultAsync();
        return project is {} ? Ok(project) : Problem(title: "Project not found", statusCode: 404);
    }
    
    [HttpPost("[controller]")]
    public async Task<IActionResult> AddProject([FromServices] ApplicationContext context, [FromServices] RequestData requestData,
        [FromBody] ProjectPostDto newProject)
    {
        Project project = newProject.Adapt<Project>();
        project.UserId = requestData.UserId!.Value;
        context.Projects.Add(project);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project.Adapt<ProjectGetDto>());
    }
    
    [HttpPatch("[controller]/{id:int}")]
    public async Task<IActionResult> PathProject(
        [FromServices] ApplicationContext context, int id,
        [FromBody] ProjectPatchDto projectPatch)
    {
        int c = await context.Projects
            .Where(t => t.Id == id)
            .BatchUpdateAsync(projectPatch.Adapt<Project>(), projectPatch.ChangedProperties.ToList());
        return c > 0 ? StatusCode(204) : Problem(title: "Project not found", statusCode: 404);
    }
}