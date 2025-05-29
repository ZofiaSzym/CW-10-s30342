using databaseFirst.Data;
using databaseFirst.DTO;
using databaseFirst.Excpetions;
using databaseFirst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace databaseFirst.DbService;

public interface IDbService
{
    public Task<IEnumerable<TripDTO>> GetTripsAsync(int page, int pageSize);
    public Task DeleteClientAsync(int clientId);
    public Task AddClientToTripAsync(ClientTripDTO clientTrip);
}

public class DbService(MasterContext data) : IDbService
{
    public async Task<IEnumerable<TripDTO>> GetTripsAsync(int page, int pageSize)
    {
        return await data.Trips
            .Include(t => t.IdCountries)
            .OrderByDescending(t => t.DateFrom)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(t => new TripDTO
            {
                IdTrip = t.IdTrip,
                Name = t.Name,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                Countries = t.IdCountries.Select(c => new CountryDto
                {
                    IdCountry = c.IdCountry,
                    Name = c.Name
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task DeleteClientAsync(int clientId)
    {
        var trips = await data.ClientTrips.FirstOrDefaultAsync(t=>t.IdClient == clientId);

        if (trips != null)
        {
            throw new OnATripsException($"Client {clientId} is associated with trips.");
        }

        var affectedRows = await data.Clients.Where(c => c.IdClient == clientId).ExecuteDeleteAsync();

        if (affectedRows == 0)
        {
            throw new NoClientException($"Client {clientId} doesn't exist.");
        }
    }

    public async Task AddClientToTripAsync(ClientTripDTO clientTrip)
    {
        //1. Sprawdzić czy klient o takim numerze PESEL już istnieje - jeśli tak zwracamy błąd
        var existingClient = await data.Clients.FirstOrDefaultAsync(c=>c.Pesel ==clientTrip.Pesel);

        if (existingClient != null)
        {
            throw new AlreadyExistsExcpetion($"Client with PESEL {clientTrip.Pesel} already exists.");
        }
        
        // Tworzymy nowego klienta
        var client = new Client
        {
            FirstName = clientTrip.FirstName,
            LastName =  clientTrip.LastName,
            Email =  clientTrip.Email,
            Telephone =  clientTrip.Telephone,
            Pesel =  clientTrip.Pesel
        };
        data.Clients.Add(client);
        await data.SaveChangesAsync();
        
        //2.  Czy klient o takim numerze PESEL jest już zapisany nadaną wycieczkę - jeśli tak, zwracamy błąd

        var clientOnTrip = await data.ClientTrips.FirstOrDefaultAsync(ct=> client.IdClient == ct.IdClient && ct.IdTrip == clientTrip.IdTrip);

        if (clientOnTrip != null)
        {
            throw new AlreadyExistsExcpetion($"Client with PESEL {clientTrip.Pesel} is on the trip.");
        }
        
        //3. Sprawdzamy czy dana wycieczka istnieje i czy DateFrom jest w przyszłości. Nie możemy zapisać się na wycieczkę, która już się odbyła.
        
        var dateOfTrip = await data.Trips.FirstOrDefaultAsync(t=>t.IdTrip==clientTrip.IdTrip);

        if (dateOfTrip == null)
        {
            throw new NoTripException($"Trip with {clientTrip.IdTrip} doesn't exist.");
        }

        if (dateOfTrip.DateFrom < DateTime.Now)
        {
            throw new NoTripException($"This trip is in the past.");
        }

        var ctNew = new ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = clientTrip.IdTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = clientTrip.PaymentDate == null ? null : clientTrip.PaymentDate
        };
        data.ClientTrips.Add(ctNew);
        await data.SaveChangesAsync();
    }
}