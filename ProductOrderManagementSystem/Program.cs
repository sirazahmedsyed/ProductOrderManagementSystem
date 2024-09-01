using Microsoft.EntityFrameworkCore;
using ProductOrderManagementSystem.Infrastructure.DBContext;
using ProductOrderManagementSystem.Infrastructure.Middleware;
using ProductOrderManagementSystem.Infrastructure.Profiles;
using ProductOrderManagementSystem.Infrastructure.Repositories;
using ProductOrderManagementSystem.Infrastructure.Services;
using ProductOrderManagementSystem.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql("Host=dpg-cr61e9jtq21c73b7oq00-a.oregon-postgres.render.com;Port=5432;Database=order_management_system_db;Username=order_processor;Password=jy925Zt3BIhgWmUQFqMaXq3azpnfih1Y;SSL Mode=Require;Trust Server Certificate=true");
});



builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddMemoryCache();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.MapControllers();

app.Run();