using Application;
using DotNetEnv;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Env.Load();

        ConfigureServices(builder.Services);

        int boardSize = int.Parse(Environment.GetEnvironmentVariable("BOARD_SIZE") ?? "3");
        int winLength = int.Parse(Environment.GetEnvironmentVariable("WIN_LENGTH") ?? "3");
        double swapChance = double.Parse(Environment.GetEnvironmentVariable("ENEMY_MOVE") ?? "0.1", CultureInfo.InvariantCulture);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "TicTacToe API");
                options.RoutePrefix = string.Empty;
            });
        }

        using var scope = app.Services.CreateScope();
        using var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await MigrateDatabase(appDbContext);

        app.UseHttpsRedirection();
        app.UseExceptionHandler();
        app.MapGet("/health", () => Results.Ok());
        app.MapControllers();

        await app.RunAsync();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__Default");

        services
            .AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://tools.ietf.org/html/rfc7807",
                        Title = "Некорректный запрос"
                    };
                    return new BadRequestObjectResult(problemDetails);
                };
            });
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddInfrastructure(connStr);
        services.AddApplication();
    }

    public static async Task MigrateDatabase(AppDbContext context)
    {
        await context.Database.MigrateAsync();
        await context.SaveChangesAsync();
    }
}