using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Test1.Models;
using Test1.Services;

namespace Test1.Controllers;

    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;

        public AppointmentsController(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentDetails(int id)
        {
            try
            {
                var res = await _databaseService.GetAppointmentByIDAsync(id);
                return Ok(res);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAppointment([FromBody] AddAppointmentDTO appointment)
        {
            try
            {
                await _databaseService.AddAppointmentAsync(appointment);
                return CreatedAtAction(nameof(GetAppointmentDetails), new { appointment.appointment_id }, null);
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
        }
    } 