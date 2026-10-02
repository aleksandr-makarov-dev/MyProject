using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyProject.WebApi.Application;
using MyProject.WebApi.Domain.Users;
using MyProject.WebApi.Infrastructure;
using MyProject.WebApi.Infrastructure.Persistence;
using MyProject.WebApi.Presentation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddInfrastructureLayer(builder.Configuration);
builder.Services.AddApplicationLayer();
builder.Services.AddPresentationLayer();

var app = builder.Build();

// TODO: refactor later
await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

    await dbContext.Database.MigrateAsync();

    if (!await roleManager.RoleExistsAsync(Roles.Administrator))
    {
        await roleManager.CreateAsync(new Role(Roles.Administrator));
    }

    if (!await roleManager.RoleExistsAsync(Roles.User))
    {
        await roleManager.CreateAsync(new Role(Roles.User));
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();