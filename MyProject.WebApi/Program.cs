using MyProject.WebApi.Infrastructure.Authentication;
using MyProject.WebApi.Infrastructure.Identity;
using MyProject.WebApi.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddPersistenceLayer(builder.Configuration);
builder.Services.AddIdentityLayer();
builder.Services.AddAuthenticationLayer(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();