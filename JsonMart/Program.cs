using JsonMart.Context;
using JsonMart.Services;
using JsonMart.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


var services = builder.Services;
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddLogging();

// Scoped
services.AddScoped<IOrderService, OrderService>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<IProductService, ProductService>();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnecting");
if (connectionString == null)
{
    throw new NullReferenceException(nameof(connectionString));
}
services.AddDbContext<JsonMartDbContext>(o => o.UseMySQL(connectionString));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();
app.Run();