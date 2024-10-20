using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApiApp.Data;
using MyApiApp.Models;
using System.Security.Claims;

namespace MyApiApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly ApplicationDbContext _userContext;
        private readonly ContactDbContext _contactContext;

        public ContactsController(ApplicationDbContext userContext, ContactDbContext contactContext)
        {
            _userContext = userContext;
            _contactContext = contactContext;
        }

        // Добавление нового контакта
        [HttpPost]
        public async Task<IActionResult> AddContact([FromBody] Contact contact)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int useradd))
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            var user = await _userContext.Users.FindAsync(useradd);
            if (user == null)
            {
                return Unauthorized("Пользователь не найден.");
            }

            contact.UserAdd = useradd;

            _contactContext.Contacts.Add(contact);
            await _contactContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, contact);
        }

        // Получение всех контактов пользователя
        [HttpGet]
        public async Task<IActionResult> GetAllContacts()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int useradd))
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            var user = await _userContext.Users.FindAsync(useradd);
            if (user == null)
            {
                return Unauthorized("Пользователь не найден.");
            }

            var contacts = await _contactContext.Contacts.Where(c => c.UserAdd == useradd).ToListAsync();
            return Ok(contacts);
        }

        // Получение одного контакта по ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContact(int id)
        {
            var contact = await _contactContext.Contacts.FindAsync(id);

            if (contact == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int useradd) || contact.UserAdd != useradd)
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            return Ok(contact);
        }

        // Удаление контакта по ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            var contact = await _contactContext.Contacts.FindAsync(id);

            if (contact == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int useradd) || contact.UserAdd != useradd)
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            _contactContext.Contacts.Remove(contact);
            await _contactContext.SaveChangesAsync();

            return NoContent();
        }

        // Обновление информации о контакте по ID
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateContact(int id, [FromBody] Contact updatedContact)
        {
            var contact = await _contactContext.Contacts.FindAsync(id);

            if (contact == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int useradd) || contact.UserAdd != useradd)
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            contact.Name = updatedContact.Name ?? contact.Name;
            contact.PhoneNumber = updatedContact.PhoneNumber ?? contact.PhoneNumber;
            contact.Email = updatedContact.Email ?? contact.Email;
            contact.Address = updatedContact.Address ?? contact.Address;

            await _contactContext.SaveChangesAsync();

            return Ok(contact);
        }

        // Поиск контактов по строковому запросу
        [HttpPost("search")]
        public async Task<IActionResult> SearchContacts([FromBody] string query)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int useradd))
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            // Используем userContext для поиска пользователя
            var user = await _userContext.Users.FindAsync(useradd);
            if (user == null)
            {
                return Unauthorized("Пользователь не найден.");
            }

            // Поиск по контактам пользователя, которые содержат запрос в имени, телефоне или email
            var contacts = await _contactContext.Contacts
                .Where(c => c.UserAdd == useradd && 
                            (c.Name.Contains(query) || 
                             c.PhoneNumber.Contains(query) || 
                             (c.Email != null && c.Email.Contains(query))))
                .ToListAsync();

            return Ok(contacts);
        }
    }
}
