using Contacts.Data;
using Contacts.Models;
using Contacts.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Contacts.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   [Authorize]
public class ContactsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ContactsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Contacts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContactResponseDto>>> GetContacts()
    {
        var contacts = await _context.Contacts
            .Select(c => new ContactResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Company = c.Company,
                Notes = c.Notes,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
            
        return contacts;
    }

    // GET: api/Contacts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ContactResponseDto>> GetContact(int id)
    {
        var contact = await _context.Contacts
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (contact == null)
        {
            return NotFound();
        }

        var contactDto = new ContactResponseDto
        {
            Id = contact.Id,
            Name = contact.Name,
            Email = contact.Email,
            Phone = contact.Phone,
            Company = contact.Company,
            Notes = contact.Notes,
            CreatedAt = contact.CreatedAt
        };

        return contactDto;
    }

    // POST: api/Contacts
    [HttpPost]
    public async Task<ActionResult<ContactResponseDto>> CreateContact(ContactCreateDto contactDto)
    {
        var contact = new Contact
        {
            Name = contactDto.Name,
            Email = contactDto.Email,
            Phone = contactDto.Phone,
            Company = contactDto.Company,
            Notes = contactDto.Notes,
            CreatedAt = DateTime.Now,
            OwnerId = "demo-user-id" // Hardcoded for now
        };

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        var responseDto = new ContactResponseDto
        {
            Id = contact.Id,
            Name = contact.Name,
            Email = contact.Email,
            Phone = contact.Phone,
            Company = contact.Company,
            Notes = contact.Notes,
            CreatedAt = contact.CreatedAt
        };

        return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, responseDto);
    }

    // PUT and DELETE methods...
}
}