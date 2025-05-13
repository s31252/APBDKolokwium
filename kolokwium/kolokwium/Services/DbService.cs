using System.Data.Common;
using kolokwium.Exceptions;
using kolokwium.Models;
using Microsoft.Data.SqlClient;

namespace kolokwium.Services;

public class DbService : IDbService
{
    private readonly string _connectionString =
        "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;";

    public async Task<GuestBookingsDto> GetBookingsByIdAsync(int bookingId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand();
        command.Connection = connection;
        await connection.OpenAsync();

        command.CommandText =
            @"SELECT B.date,G.first_name,G.last_name,G.date_of_birth,E.first_name,E.last_name,E.employee_number,A.name,A.price,BA.amount FROM Booking B
                JOIN Guest G ON B.guest_id = G.guest_id
                JOIN Employee E ON B.employee_id = E.employee_id
                JOIN Booking_Attraction BA ON B.booking_id = BA.booking_id
                JOIN Attraction A ON BA.attraction_id = A.attraction_id
                WHERE B.booking_id=@bookingId";
        command.Parameters.AddWithValue("@bookingId", bookingId);

        await using var reader = await command.ExecuteReaderAsync();
        GuestBookingsDto? booking = null;

        while (await reader.ReadAsync())
        {
            if (booking is null)
            {
                booking = new GuestBookingsDto()
                {
                    Date = reader.GetDateTime(0),
                    Guest = new GuestDto
                    {
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        DateOfBirth = reader.GetDateTime(3),
                    },
                    Employee = new EmployeeDto
                    {
                        FirstName = reader.GetString(4),
                        LastName = reader.GetString(5),
                        EmployeeNumber = reader.GetString(6),
                    }
                };
            }

            booking.Attractions.Add(new AttractionDto
            {
                Name = reader.GetString(7),
                Price = reader.GetDecimal(8),
                Amount = reader.GetInt32(9),
            });
        }

        if (booking is null)
        {
            throw new NotFoundException("Booking with that id not found");
        }

        return booking;
    }

    public async Task AddNewBookingAsync(CreateBookingRequestDto createBookingRequest)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand();
        command.Connection = connection;
        await connection.OpenAsync();
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        try
        {
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Guest WHERE guest_id = @guestId";
            command.Parameters.AddWithValue("@guestId", createBookingRequest.GuestId);
            
            if (await command.ExecuteScalarAsync() == null)
                throw new NotFoundException($"Guest with id: {createBookingRequest.GuestId} not found");
            command.Parameters.Clear();

            command.CommandText = "SELECT employee_id FROM Employee WHERE employee_number = @employeeNumber";
            command.Parameters.AddWithValue("@employeeNumber", createBookingRequest.EmployeeNumber);
            
            var employeeId = await command.ExecuteScalarAsync();

            if (employeeId is null)
                throw new NotFoundException($"Employee with employee number: {createBookingRequest.EmployeeNumber} not found");
            command.Parameters.Clear();

            command.CommandText = @"INSERT INTO Booking (booking_id,guest_id,employee_id,date)
            VALUES(@bookingId,@guestId,@employeeId,@date)";
            command.Parameters.AddWithValue("@bookingId", createBookingRequest.BookingId);
            command.Parameters.AddWithValue("@guestId", createBookingRequest.GuestId);
            command.Parameters.AddWithValue("@employeeId", employeeId);
            command.Parameters.AddWithValue("@date", DateTime.Now);
            
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception)
            {
                throw new ConflictException("Booking with that id already exists");
            }
            
            
            foreach (var attraction in createBookingRequest.Attractions)
            {
                command.Parameters.Clear();
                command.CommandText = "SELECT attraction_id FROM Attraction WHERE name = @atractionName";
                command.Parameters.AddWithValue("@atractionName", attraction.Name);
                
                var attractionId = await command.ExecuteScalarAsync();

                if (attractionId is null)
                    throw new NotFoundException($"Attraction {attraction.Name} not found");
                

                command.Parameters.Clear();

                command.CommandText = @"INSERT INTO Booking_Attraction(booking_id, attraction_id, amount)
                VALUES(@bookingId, @attractionId, @amount)";
                command.Parameters.AddWithValue("@bookingId", createBookingRequest.BookingId);
                command.Parameters.AddWithValue("@attractionId", attractionId);
                command.Parameters.AddWithValue("@amount", attraction.Amount);
                
                await command.ExecuteNonQueryAsync();
            }
            
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}