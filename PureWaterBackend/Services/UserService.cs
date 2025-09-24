using Microsoft.EntityFrameworkCore;
using PureWaterBackend.Models;
using System.Security.Claims;



namespace PureWaterBackend.Services;

public class UserService
{
    private readonly WaterDbContext _context;

    public UserService(WaterDbContext context)
    {
        _context = context;
    }
    public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal user)
    {
        var googleId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
    }
}