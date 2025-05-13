using kolokwium.Exceptions;
using kolokwium.Models;
using kolokwium.Services;
using Microsoft.AspNetCore.Mvc;

namespace kolokwium.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class BookingsController : ControllerBase
{
    
    private readonly IDbService _dbService;

    public BookingsController(IDbService dbService)
    {
        _dbService = dbService;
    }
    
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetModelAsync(int id)
    {
        try
        {
            var result = await _dbService.GetBookingsByIdAsync(id);
            return Ok(result);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddNewModelAsync(CreateBookingRequestDto createModelRequestDto)
    {
        if (!createModelRequestDto.Attractions.Any())
        {
            return NotFound("Attractions are required");
        }
        try
        {
            await _dbService.AddNewBookingAsync(createModelRequestDto);
            return Ok($"Booking with id {createModelRequestDto.BookingId} has been successfully added");
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}