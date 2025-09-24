using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PureWaterBackend.Models;


namespace PureWaterBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly WaterDbContext _context;

    public UserController(WaterDbContext context)
    {
        _context = context;
    }

    // GET: api/Users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.ToListAsync();
    }

    // GET: api/Users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    // POST: api/Users   // i will create it later because I use only google login


    //soft DELETE: api/Users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        try
        {
            user.IsDeleted = true;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException) { throw;}

    }

    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}