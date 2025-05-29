using databaseFirst.DbService;
using databaseFirst.Excpetions;
using Microsoft.AspNetCore.Mvc;

namespace databaseFirst.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientController(IDbService service):ControllerBase
{
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient([FromRoute] int id)
    {
        try
        {
            await service.DeleteClientAsync(id);
            return NoContent();
        }
        catch (NoClientException e)
        {
            return NotFound(e.Message);
        }
        
    }
    
}