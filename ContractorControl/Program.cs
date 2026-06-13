using ContractorControl.Api.Middleware;
using ContractorControl.Application.Interfaces;
using ContractorControl.Application.Services;
using ContractorControl.Infrastructure.Data;
using ContractorControl.Infrastructure.Interceptors;
using ContractorControl.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
builder.WebHost.UseUrls($"http://localhost:{builder.Configuration["AppSettings:Port"] ?? "6767"}");

builder.Services.AddScoped<AuditableEntityInterceptor>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(new AuditableEntityInterceptor()));

builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
builder.Services.AddScoped<ICrudService, CrudService>();
builder.Services.AddScoped<InitService>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapControllers();
app.Run();
