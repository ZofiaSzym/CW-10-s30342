namespace databaseFirst.DTO;

public class TripDTO
{
    public int IdTrip { get; set; }
    public string Name { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public List<CountryDto> Countries { get; set; }
}

public class CountryDto
{
    public int IdCountry { get; set; }
    public string Name { get; set; }
}