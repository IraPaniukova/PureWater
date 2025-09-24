using Microsoft.EntityFrameworkCore;
using PureWaterBackend.Controllers;
using PureWaterBackend.Models;
using Microsoft.AspNetCore.Mvc;

public class WaterControllerTest
{
    private WaterDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<WaterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new WaterDbContext(options);
    }

    [Fact]
    public async Task GetWaterTable_Returns_AllWater()
    {
        var context = GetInMemoryContext();

        // Mock data
        context.Water.Add(new Water { Id = 1, UserId = 1, AmountMl = 500, Date = new DateOnly(2025, 9, 24) });
        context.Water.Add(new Water { Id = 2, UserId = 1, AmountMl = 300, Date = new DateOnly(2025, 9, 23) });
        await context.SaveChangesAsync();

        var controller = new WaterController(context);

        // Act
        var result = await controller.GetWaterTable();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Water>>>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<Water>>(actionResult.Value);
        Assert.Equal(2, Enumerable.Count(list));
    }
     [Fact]
    public async Task GetWaterByUser_ReturnsExistingWater()
    {
        var context = GetInMemoryContext();
        var user = new User { Id = 1, GoogleId = "g1" };
        var water = new Water { Id = 1, UserId = 1, AmountMl = 500, Date = DateOnly.FromDateTime(DateTime.Now) };
        context.Users.Add(user);
        context.Water.Add(water);
        await context.SaveChangesAsync();

        var controller = new WaterController(context);
        var result = await controller.GetWaterByUser(1);

        var okResult = Assert.IsType<ActionResult<Water>>(result);
        var returnedWater = Assert.IsType<Water>(okResult.Value);
        Assert.Equal(500, returnedWater.AmountMl);
    }

    [Fact]
    public async Task PostWater_AddsNewWater_WhenNoneExists()
    {
        var context = GetInMemoryContext();
        var user = new User { Id = 1, GoogleId = "g1" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = new WaterController(context);
        var result = await controller.PostWater(300, DateTime.Now, "g1");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var dto = Assert.IsType<WaterDto>(okResult.Value);
        Assert.Equal(300, dto.AmountMl);
    }

    [Fact]
    public async Task GetWaterByDay_ReturnsBadRequest_WhenDateInFuture()
    {
        var context = GetInMemoryContext();
        var controller = new WaterController(context);

        var futureDate = DateTime.Now.AddDays(1);
        var result = await controller.GetWaterByDay(futureDate, "g1");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Date cannot be in the future", badRequest.Value);
    }

    [Fact]
    public async Task GetWaterByDay_ReturnsNotFound_WhenNoWater()
    {
        var context = GetInMemoryContext();
        var user = new User { Id = 1, GoogleId = "g1" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = new WaterController(context);
        var result = await controller.GetWaterByDay(DateTime.Now, "g1");

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
