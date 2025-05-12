using Microsoft.Data.SqlClient;
using Test1.Models;

namespace Test1.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;;
    }

    public async Task<AppointmentDTO> GetAppointmentByIDAsync(int appointment_id)
    {
        await using SqlConnection con = new SqlConnection(_connectionString);
        await using SqlCommand com = new SqlCommand();
        
        com.Connection = con;
        com.CommandText =
            @"SELECT a.Date, p.FirstName, p.LastName, p.DateOfBirth, d.DoctorId, d.PWZ, s.Name, s.Fee
            FROM Appointment a
            JOIN Patient p ON a.PatientId = p.PatientId
            JOIN Doctor d ON a.DoctorId = d.DoctorId
            JOIN Appointment_Service aps ON a.AppointmentId = aps.AppointmentId
            JOIN Service s ON aps.ServiceId = s.ServiceId
            WHERE a.AppointmentId = @appointment_id;";
        
        await con.OpenAsync();
        
        com.Parameters.AddWithValue("@appointment_id", appointment_id);
        var reader = await com.ExecuteReaderAsync();

        if (!reader.HasRows)
        {
            throw new ApplicationException("Appointment not found");
        }

        var appointmentDTO = new AppointmentDTO()
        {
            services = new List<ServiceDTO>()
        };

        while (await reader.ReadAsync())
        {
            appointmentDTO.date = reader.GetDateTime(0);
            appointmentDTO.patient = new PatientDTO()
            {
                first_name = reader.GetString(1),
                last_name = reader.GetString(2),
                date_of_birth = reader.GetDateTime(3),
            };

            appointmentDTO.doctor = new DoctorDTO()
            {
                doctor_id = reader.GetInt32(4),
                PWZ = reader.GetString(5),
            };

            appointmentDTO.services.Add(new ServiceDTO()
            {
                name = reader.GetString(6),
                service_fee = reader.GetDecimal(7)
            });
        }
        
        return appointmentDTO;

    }

    public async Task AddAppointmentAsync(AddAppointmentDTO appointment)
    {
        using var con = new SqlConnection(_connectionString);
        await con.OpenAsync();
        
        using var transaction = con.BeginTransaction();

        try
        {
            var com1 = new SqlCommand("SELECT COUNT(*) FROM Appointment WHERE AppointmentId = @id", con, transaction);
            com1.Parameters.AddWithValue("@id", appointment.appointment_id);
            var count = (int) await com1.ExecuteScalarAsync();
            if (count > 0)
            {
                throw new ApplicationException("Appointment already exists");
            }
            
            var com2 = new SqlCommand("SELECT COUNT(*) FROM Patient WHERE PatientId = @id", con, transaction);
            com2.Parameters.AddWithValue("@id", appointment.patient_id);
            if ((int)await com2.ExecuteScalarAsync() == 0)
            {
                throw new ApplicationException("Patient does not exist");
            }

            var com3 = new SqlCommand("SELECT DoctorId FROM Doctor WHERE PWZ = @pwz", con, transaction);
            com3.Parameters.AddWithValue("@pwz", appointment.PWZ);
            object doctor_id = await com3.ExecuteScalarAsync();
            if (doctor_id == null)
            {
                throw new ApplicationException("Doctor does not exist");
            }

            int doctorId = (int)doctor_id;
            
            var service_ids = new List<int>();
            foreach (var service in appointment.services)
            {
                var scom = new SqlCommand("SELECT ServiceId FROM Service WHERE Name = @name AND Fee = @fee", con, transaction);
                scom.Parameters.AddWithValue("@name", service.name);
                scom.Parameters.AddWithValue("@fee", service.service_fee);
                object service_id = await scom.ExecuteScalarAsync();
                if (service_id == null)
                    throw new ApplicationException("Service does not exist");
                service_ids.Add((int)service_id);
            }

            var insertcom = new SqlCommand("INSERT INTO Appointment (AppointmentId, Date, PatientId, DoctorId) VALUES (@id, @date, @patientId, @doctorId)", con, transaction);
            insertcom.Parameters.AddWithValue("@id", appointment.appointment_id);
            insertcom.Parameters.AddWithValue("@date", DateTime.Now);
            insertcom.Parameters.AddWithValue("@patient_id", appointment.patient_id);
            insertcom.Parameters.AddWithValue("@doctor_id", doctor_id);
            await insertcom.ExecuteNonQueryAsync();

            foreach (var service_id in service_ids)
            {
                var insertcom2 = new SqlCommand("INSERT INTO Appointment_Service (AppointmentId, ServiceId) VALUES (@appointmentId, @serviceId)", con, transaction);
                insertcom2.Parameters.AddWithValue("@appointmentId", appointment.appointment_id);
                insertcom2.Parameters.AddWithValue("@service_id", service_id);
                await insertcom2.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw;
        }
    }
}