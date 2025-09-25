using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PureWaterBackend.Models;
using System.Security.Claims;



namespace PureWaterBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WaterController : ControllerBase
{
    private readonly WaterDbContext _context;

    public WaterController(WaterDbContext context)
    {
        _context = context;
    }
    private async Task<int?> CurrentUserIdAsync()
    {
        var googleId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("No logged-in user");
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
        return user?.Id;   
    }
      private WaterDto ToWaterDto(Water water) =>
        new()
        {
            Id = water.Id,
            UserId = water.UserId,
            Date = water.Date,
            AmountMl = water.AmountMl
        };
    // Other potential endpoints:
        // GET /api/water/range?start=YYYY-MM-DD&end=YYYY-MM-DD
        // Returns water data for a period (for graphs)

        // DELETE /api/water/day?date=YYYY-MM-DD
        // Deletes water data for a specific day (e.g., user wants to reset the day)

        // GET /api/water/summary?period=week/month
        // Returns weekly or monthly summary, calculated from range   

    //POST: api/Water
    // [Authorize] //TODO: need to add authorization later for googleId
    [HttpPost]
    public async Task<ActionResult<WaterDto>> PostWater(int ml, DateTime date, string googleId)
    {
        DateOnly dateOnly = DateOnly.FromDateTime(date);
        if (dateOnly > DateOnly.FromDateTime(DateTime.Now))  return BadRequest("Date cannot be in the future");
         try
        {
            //TODO: need to add a check for the user is a current user later. Remove googleId from the input, replace next 3 lines with with the following:
            // int? userId =await CurrentUserIdAsync();
            // if (userId == null) return Unauthorized(); 
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
            if (user == null) return Unauthorized();    
            int userId = user.Id;

            Water? water = await _context.Water.FirstOrDefaultAsync(w => w.UserId == userId && w.Date == dateOnly);
            if (water != null)
            {
                if(water.AmountMl + ml < 0) 
                {
                    return BadRequest
                    ($"The total amount of water cannot be negative: {water.AmountMl + ml} ({water.AmountMl}{ml})");
                }
                if (water.AmountMl + ml > 10000)
                {
                    return BadRequest("Cannot exceed 10 litres per day - can be dangerous for a person's health");
                    //for the case the frontend can add an advise to see a doctor
                }
                
                water.AmountMl += ml;
                _context.Entry(water).State = EntityState.Modified;
            }

            else
            {
                 if(ml < 0) 
                {
                    return BadRequest
                    ($"The amount of water cannot be negative.");
                }
                water = new()
                {
                    UserId = userId,
                    AmountMl = ml,
                    Date = dateOnly
                };
                _context.Water.Add(water);}
        
                await _context.SaveChangesAsync();
                return Ok(ToWaterDto(water));
                // Returns only necessary fields to avoid self-referencing loop if i return just water (Water → User → Water → ...)
        }
        catch (DbUpdateException)
        {
            throw;
        }
    }
    // GET /api/water/day?date=date=YYYY-MM-DD
    [HttpGet("day")]
    public async Task<ActionResult<WaterDto>> GetWaterByDay(DateTime date, string googleId)
    {
         DateOnly dateOnly = DateOnly.FromDateTime(date);
        if (dateOnly > DateOnly.FromDateTime(DateTime.Now))  return BadRequest ("Date cannot be in the future");
        if (dateOnly < DateOnly.FromDateTime(DateTime.Now).AddDays(-90)) return BadRequest("Date is too far in the past. Can only get data for the last 30 days.");
         try
        {
            //TODO: need to add a check for the user is a current user later. Remove googleId from the input, replace next 3 lines with with the following:
            // int? userId =await CurrentUserIdAsync();
            // if (userId == null) return Unauthorized(); 
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
            if (user == null) return Unauthorized();
            int userId = user.Id;

            var water = await _context.Water.FirstOrDefaultAsync(w => w.UserId == userId && w.Date == dateOnly);
            if (water == null) return NotFound();
            return Ok(ToWaterDto(water));

        }
        catch (Exception)
        {
            throw;
        }
    }

    // GET: api/Water
        [HttpGet]
    public async Task<ActionResult<IEnumerable<Water>>> GetWaterTable()
    {
        return await _context.Water.ToListAsync(); //probaly better to use dto without water Id
    }

    // GET: api/Water/5
    [HttpGet("{googleId}")]
    public async Task<ActionResult<Water>> GetWaterByUser(int googleId)
    {
        var water = await _context.Water.FindAsync(googleId);

        if (water == null)
        {
            return NotFound();
        }

        return Ok(water);
    }   
    
    private bool WaterExists(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}