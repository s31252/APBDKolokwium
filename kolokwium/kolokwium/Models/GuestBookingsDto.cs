namespace kolokwium.Models;

public class GuestBookingsDto
{
    public DateTime Date { get; set; }
    public GuestDto Guest { get; set; }
    public EmployeeDto Employee{get;set;}
    public List<AttractionDto> Attractions { get; set; } = [];

}

public class GuestDto
{
    public string FirstName { get; set; }=String.Empty;
    public string LastName { get; set; }=String.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class EmployeeDto
{
    public string FirstName { get; set; }=String.Empty;
    public string LastName { get; set; }=String.Empty;
    public string EmployeeNumber { get; set; }=String.Empty;
}

public class AttractionDto
{
    public string Name { get; set; }=String.Empty;
    public decimal Price { get; set; }
    public int Amount { get; set; }
}