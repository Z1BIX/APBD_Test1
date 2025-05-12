using Test1.Models;

namespace Test1.Services;

public interface IDatabaseService
{
    Task<AppointmentDTO> GetAppointmentByID(int id);
    Task AddAppointment(AddAppointmentDTO appointment);
}