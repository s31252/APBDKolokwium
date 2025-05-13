using kolokwium.Models;

namespace kolokwium.Services;

public interface IDbService
{
    Task<GuestBookingsDto> GetBookingsByIdAsync(int modelId);
    Task AddNewBookingAsync(CreateBookingRequestDto createBookingRequest);
}