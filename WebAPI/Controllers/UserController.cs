using Domain.DTO;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using WebAPI.Services.IServices;


namespace WebApi29.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize] // 👈 Protege todo el controlador
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;

        public UserController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userServices.GetUsers();
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userServices.GetByIdUser(id);
            if (user == null)
                return NotFound($"No se encontró un usuario con ID {id}.");
            return Ok(user);
        }

        [HttpPost("crear")]
        public async Task<IActionResult> PostUser([FromBody] UserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdUser = await _userServices.CreateUser(request);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.PKUser }, createdUser);
        }

        [HttpPut("editar/{id:int}")]
        public async Task<IActionResult> PutUser(int id, [FromBody] UserRequest request)
        {
            if (id != request.PKUser)
                return BadRequest("El ID en la URL no coincide con el ID del cuerpo.");

            var updatedUser = await _userServices.EditUser(request);
            if (updatedUser == null)
                return NotFound($"No se pudo actualizar el usuario con ID {id}.");

            return Ok(updatedUser);
        }

        [HttpDelete("eliminar/{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _userServices.DeleteUser(id);
            if (!deleted)
                return NotFound($"No se encontró el usuario con ID {id} para eliminar.");

            return Ok(new { message = $"Usuario con ID {id} eliminado correctamente." });
        }
    }
}