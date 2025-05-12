using Test1.Models;

namespace Test1.Services;

public interface IDatabaseService
{
    Task<AppointmentDTO> GetAppointmentByIDAsync(int id);
    Task AddAppointmentAsync(AddAppointmentDTO appointment);
}