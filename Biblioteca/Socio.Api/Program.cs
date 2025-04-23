using Socio.Api.IoC;
using Socio.Api.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//my services
builder.Services.AddServices();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddLogging();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
app.UseSwagger();
app.UseSwaggerUI();
app.Logger.LogInformation("Application started");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
