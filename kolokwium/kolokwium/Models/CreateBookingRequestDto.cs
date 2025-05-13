namespace kolokwium.Models;

public class CreateBookingRequestDto

{
    public int BookingId { get; set; }
    public int GuestId { get; set; }
    public string EmployeeNumber { get; set; }=String.Empty;
    public List<AttractionToInsertDto> Attractions { get; set; } = [];
}

public class AttractionToInsertDto
{
    public string Name { get; set; }=String.Empty;
    public int Amount { get; set; }
}