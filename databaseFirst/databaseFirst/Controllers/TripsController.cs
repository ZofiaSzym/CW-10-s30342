using databaseFirst.DbService;
using databaseFirst.DTO;
using databaseFirst.Excpetions;
using databaseFirst.Models;
using Microsoft.AspNetCore.Mvc;

namespace databaseFirst.Controllers;

[ApiController]
[Route("[controller]")]
public class TripsController(IDbService service):ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTrips(
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 10)
    {
        return Ok(await service.GetTripsAsync(page, pageSize));
    }

    [HttpPost("{idTrips}/clients")]
    public async Task<IActionResult> AddClientToTrip(ClientTripDTO clientTripBody)
    {
        try
        {
           await service.AddClientToTripAsync(clientTripBody);
           return NoContent();
        }
        catch (NoTripException e)
        {
            return NotFound(e.Message);
        }
    }
    
}