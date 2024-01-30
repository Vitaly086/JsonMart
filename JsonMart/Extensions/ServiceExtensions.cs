using JsonMart.Context;
using JsonMart.Services;
using JsonMart.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JsonMart.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddLogging();
            
            // Scoped
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProductService, ProductService>();
        }
        
        public static void AddDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<JsonMartDbContext>(options => options.UseMySQL(connectionString));
        }
    }
}