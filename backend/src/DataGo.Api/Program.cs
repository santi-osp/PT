using System.Text.Json.Serialization;
using DataGo.Api;
using DataGo.Application;
using DataGo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required");

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<CreateResidentialCustomerHandler>();
builder.Services.AddScoped<GetResidentialCustomerHandler>();
builder.Services.AddScoped<SearchResidentialCustomersHandler>();
builder.Services.AddScoped<UpdateResidentialCustomerHandler>();
builder.Services.AddScoped<RetireResidentialCustomerHandler>();
builder.Services.AddScoped<GetCentersHandler>();
builder.Services.AddScoped<SearchNeighborhoodsHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ApiExceptionMiddleware>();
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

public partial class Program;
