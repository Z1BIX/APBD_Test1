namespace Test1.Models;

public class AppointmentDTO
{
    public DateTime date { get; set; }
    public PatientDTO patient { get; set; }
    public DoctorDTO doctor { get; set; }
    public List<ServiceDTO> services { get; set; }
}