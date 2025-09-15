using FileUploadPerformance.ApiService;
using Microsoft.Extensions.Logging.Console;
using Scalar.AspNetCore;

var logger = LoggerFactory.Create(lb =>
{
    lb.AddSimpleConsole(con =>
    {
        con.ColorBehavior = LoggerColorBehavior.Enabled;
        con.SingleLine = true;
    });
}).CreateLogger<Program>();

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services
    .Configure<SystemSettings>(builder.Configuration)
    .AddSingleton<IFileStorageService, FileStorageService>();
    


// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/dev/", (options, httpContext) =>
    {
        const string prefixHeader = "X-Forwarded-Prefix";
        if (!httpContext.Request.Headers.TryGetValue(prefixHeader, out var forwardPrefix))
        {
            options.Servers = [];
        }
        else
        {
            var serverUrl = forwardPrefix.ToString() ?? throw new ArgumentNullException("forwardPrefix.ToString()");


            logger.LogInformation("request forwarded Prefix: {prefix}", serverUrl);

            options.Servers = [new ScalarServer(serverUrl)];
            logger.LogInformation("Starting Scalar Server at {path}", serverUrl);
        }

    });
}

app.MapDefaultEndpoints();

app.MapControllers();
app.Run();
