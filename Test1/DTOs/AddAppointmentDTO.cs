namespace Test1.Models;

public class AddAppointmentDTO
{
    public int appointment_id { get; set; }
    public int patient_id { get; set; }
    public int PWZ { get; set; }
    public List<ServiceDTO> services { get; set; }
}