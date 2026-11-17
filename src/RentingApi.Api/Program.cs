using System.Text.Json;
using RentingApi.Application.Abstractions;
using RentingApi.Infrastructure;
using RentingApi.Api.Errors;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.Json;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddScoped<Mediator>();
builder.Services.AddInfrastructure();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";   // ← produces "v1", "v2" (matches /openapi/v1.json)
        options.SubstituteApiVersionInUrl = true;
    })
    .AddMvc()
    .AddOpenApi(options =>
    {
        options.Document.AddOperationTransformer((operation, context, ct) =>
        {
            if (!string.IsNullOrEmpty(operation.OperationId))
            {
                operation.OperationId = char.ToLowerInvariant(operation.OperationId[0]) + operation.OperationId[1..];
            }
            return Task.CompletedTask;
        });
    });   // ← THIS is the Asp.Versioning overload, not Microsoft.AspNetCore.OpenApi

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().WithDocumentPerVersion();  // ← one document per version
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar"));
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
