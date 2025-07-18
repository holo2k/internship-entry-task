using Application.Services.Abstractions;
using Application.Services.Implementations;
using Application.Validation;
using FluentValidation;
using Infrastructure.Data;
using Infrastructure.Repository.Abstractions;
using Infrastructure.Repository.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public static class InjectionHelper
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connString)
        {
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connString));

            services.AddScoped<IMoveRepository, MoveRepository>();
            services.AddScoped<IGameRepository, GameRepository>();

            return services;
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IMoveService, MoveService>();
            services.AddScoped<IGameService, GameService>();
            services.AddScoped<IRandomProvider, DefaultRandomProvider>();

            services.AddValidatorsFromAssemblyContaining<MoveCommandValidator>();

            return services;
        }
    }
}
