using Asp.Versioning;
using Scalar.AspNetCore;
using RentingApi.Application.Abstractions;
using RentingApi.Infrastructure;
using RentingApi.Api.Errors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register custom Mediator (CQRS dispatcher)
builder.Services.AddScoped<Mediator>();

// Register Infrastructure layer (which registers ports and handlers)
builder.Services.AddInfrastructure();

// Register exception handler
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();

// Configure API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddMvc()
  .AddApiExplorer(options =>
  {
      options.GroupNameFormat = "'v'VVV";
      options.SubstituteApiVersionInUrl = true;
  });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

// Add Scalar API documentation for each API version
app.MapScalarApiReference(options =>
{
    options.WithTitle("RentingApi");
});

app.Run();
