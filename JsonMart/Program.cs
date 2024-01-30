using JsonMart.Extensions;
using JsonMart.Middlewares;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnecting");
if (connectionString == null)
{
    throw new NullReferenceException(nameof(connectionString));
}

var services = builder.Services;
services.AddApplicationServices();
services.AddDbContext(connectionString);


var app = builder.Build();

app.UseCustomExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();