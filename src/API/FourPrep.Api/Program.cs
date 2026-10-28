using Serilog;
using System.Reflection;
using FourPrep.Api.Extensions;
using FourPrep.Api.Middleware;
using FourPrep.Common.Application;
using FourPrep.Common.Infrastructure;
using FourPrep.Modules.AI.Infrastructure;
using FourPrep.Modules.Auth.Infrastructure;
using FourPrep.Modules.Chat.Infrastructure;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Academy.Infrastructure;
using FourPrep.Modules.Content.Infrastructure;
using FourPrep.Modules.Payment.Infrastructure;
using FourPrep.Modules.Storage.Infrastructure;
using FourPrep.Modules.Student.Infrastructure;
using FourPrep.Modules.Admission.Infrastructure;
using FourPrep.Modules.Agreement.Infrastructure;
using FourPrep.Common.Infrastructure.Configuration;
using FourPrep.Modules.Notification.Infrastructure;

Console.Title = "FourPrep API";

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

Assembly[] moduleApplicationAssemblies =
[
    FourPrep.Modules.Auth.Application.AssemblyReference.Assembly,
    FourPrep.Modules.Storage.Application.AssemblyReference.Assembly,
    FourPrep.Modules.Notification.Application.AssemblyReference.Assembly,
    FourPrep.Modules.Chat.Application.AssemblyReference.Assembly,
    FourPrep.Modules.Payment.Application.AssemblyReference.Assembly,
    FourPrep.Modules.Student.Application.AssemblyReference.Assembly,
    FourPrep.Modules.Content.Application.AssemblyReference.Assembly,
    FourPrep.Modules.AI.Application.AssemblyReference.Assembly,
    FourPrep.Modules.Academy.Application.AssemblyReference.Assembly,
    FourPrep.Modules.Agreement.Application.AssemblyReference.Assembly,
    FourPrep.Modules.Admission.Application.AssemblyReference.Assembly
];
builder.Services.AddApplication(moduleApplicationAssemblies);

string databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow("Database");
builder.Services.AddInfrastructure(databaseConnectionString);

builder.Services.AddAuthModule(builder.Configuration);
builder.Services.AddStorageModule(builder.Configuration);
builder.Services.AddNotificationModule(builder.Configuration);
builder.Services.AddChatModule(builder.Configuration);
builder.Services.AddPaymentModule(builder.Configuration);
builder.Services.AddStudentModule(builder.Configuration);
builder.Services.AddContentModule(builder.Configuration);
builder.Services.AddAIModule(builder.Configuration);
builder.Services.AddAcademyModule(builder.Configuration);
builder.Services.AddAgreementModule(builder.Configuration);
builder.Services.AddAdmissionModule(builder.Configuration);

builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddRateLimitPolicies();
builder.Services.AddTrustedProxies(builder.Configuration);

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.UseForwardedHeaders();

app.UseSwaggerDocumentation();

app.UseCorsPolicy();

app.UseLogContext();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.UseJobsDashboard();

app.MapHealthChecks("health");

app.MapEndpoints();

await app.RunAsync();
