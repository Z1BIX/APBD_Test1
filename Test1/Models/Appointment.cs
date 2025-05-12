namespace Test1.Models;

public class Appointment
{
    public int appointment_id { get; set; }
    public int patient_id { get; set; }
    public int doctor_id { get; set; }
    public DateTime date { get; set; }
}