namespace FourPrep.Modules.Storage.Infrastructure;

using Amazon.S3;
using Amazon.Runtime;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FourPrep.Common.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using FourPrep.Modules.Storage.Infrastructure.Jobs;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Infrastructure.Database;
using FourPrep.Modules.Storage.Infrastructure.StoredFiles;
using FourPrep.Modules.Storage.Infrastructure.ObjectStorage;
using FourPrep.Modules.Storage.Infrastructure.VirusScanning;
using FourPrep.Modules.Storage.Application.Abstractions.VirusScanning;
using FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;
using FourPrep.Modules.Storage.Application.Abstractions.Data;
using FourPrep.Modules.Storage.Infrastructure.Authentication;
using FourPrep.Modules.Storage.Application.Abstractions.Authentication;

public static class StorageModule
{
    public static IServiceCollection AddStorageModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<StorageDbContext>((_, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Storage))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<StorageDbContext>());

        services.AddScoped<IStoredFileRepository, StoredFileRepository>();

        services.AddOptions<S3StorageOptions>()
            .Bind(configuration.GetSection(S3StorageOptions.SectionName))
            .Validate(S3StorageOptions.IsComplete, S3StorageOptions.IncompleteMessage)
            .Validate(S3StorageOptions.KeepsDocumentsPrivate, S3StorageOptions.DocumentsNotPrivateMessage)
            .ValidateOnStart();
        services.AddSingleton<IAmazonS3>(sp => CreateS3Client(sp.GetRequiredService<IOptions<S3StorageOptions>>().Value));
        services.AddSingleton<IObjectStorage, S3ObjectStorage>();

        services.AddOptions<ClamAvOptions>()
            .Bind(configuration.GetSection(ClamAvOptions.SectionName))
            .Validate(ClamAvOptions.IsComplete, ClamAvOptions.IncompleteMessage)
            .ValidateOnStart();
        services.AddSingleton<IVirusScanner, ClamAvScanner>();
        services.AddScoped<IVirusScanQueue, HangfireVirusScanQueue>();

        services.AddScoped<RemoveAbandonedUploadsJob>();
        services.AddScoped<ScanUploadedFilesJob>();
        services.AddHostedService<RecurringJobsRegistration>();
    }

    private static AmazonS3Client CreateS3Client(S3StorageOptions options) =>
        new(
            new BasicAWSCredentials(options.AccessKey, options.SecretKey),
            new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                AuthenticationRegion = options.Region,
                ForcePathStyle = options.ForcePathStyle,
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
            });
}
