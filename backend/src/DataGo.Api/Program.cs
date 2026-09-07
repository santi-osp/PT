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
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddAssistantInfrastructure(new OpenRouterOptions(
    builder.Configuration["OpenRouter:ApiKey"]
        ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
        ?? Environment.GetEnvironmentVariable("OPENROUTER_KEY"),
    builder.Configuration["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1",
    builder.Configuration["OpenRouter:Model"] ?? "openai/gpt-4o-mini",
    builder.Configuration.GetValue("OpenRouter:TimeoutSeconds", 30)));
builder.Services.AddScoped<CreateResidentialCustomerHandler>();
builder.Services.AddScoped<GetResidentialCustomerHandler>();
builder.Services.AddScoped<SearchResidentialCustomersHandler>();
builder.Services.AddScoped<CountResidentialCustomersHandler>();
builder.Services.AddScoped<UpdateResidentialCustomerHandler>();
builder.Services.AddScoped<RetireResidentialCustomerHandler>();
builder.Services.AddScoped<GetCentersHandler>();
builder.Services.AddScoped<SearchNeighborhoodsHandler>();
builder.Services.AddScoped<AssistantMessageHandler>();
builder.Services.AddScoped<ConfirmAssistantActionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ApiExceptionMiddleware>();
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Frontend");

app.MapControllers();

app.Run();

public partial class Program;
