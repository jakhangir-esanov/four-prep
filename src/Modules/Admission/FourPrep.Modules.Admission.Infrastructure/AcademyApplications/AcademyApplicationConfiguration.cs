namespace FourPrep.Modules.Admission.Infrastructure.AcademyApplications;

using Microsoft.EntityFrameworkCore;
using FourPrep.Common.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FourPrep.Modules.Admission.Domain.ReportedScores;
using FourPrep.Modules.Admission.Domain.AcademyApplications;

internal sealed class AcademyApplicationConfiguration : IEntityTypeConfiguration<AcademyApplication>
{
    private static readonly string OpenOnly =
        $"status IN ({(int)AcademyApplicationStatus.Submitted}, {(int)AcademyApplicationStatus.Accepted})";
    private const string VersionProperty = "Version";

    public void Configure(EntityTypeBuilder<AcademyApplication> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudentName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Interests).HasJsonbConversion().IsRequired();
        builder.Property(x => x.ContactChannels).HasJsonbConversion().IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.Property(x => x.TelegramUsername).HasMaxLength(32);
        builder.Property(x => x.MeetingLanguage).HasMaxLength(8).IsRequired();
        builder.Property(x => x.Question).HasMaxLength(2000);
        builder.Property(x => x.DeclineReason).HasMaxLength(500);
        builder.Property(x => x.SubmittedOnUtc).IsRequired();
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.UpdatedOnUtc).IsRequired();
        builder.Property<uint>(VersionProperty).IsRowVersion();

        builder.Ignore(x => x.IsOpen);

        builder.OwnsMany(x => x.Scores, score =>
        {
            score.ToTable("reported_scores");
            score.WithOwner().HasForeignKey("AcademyApplicationId");
            score.HasKey("AcademyApplicationId", nameof(ReportedScore.Exam));
            score.Property(x => x.Score).HasPrecision(6, 1);
        });

        builder.Navigation(x => x.Scores).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.StudentUserId).IsUnique().HasFilter(OpenOnly);
        builder.HasIndex(x => new { x.StudentUserId, x.SubmittedOnUtc });
        builder.HasIndex(x => new { x.Status, x.SubmittedOnUtc });
    }
}
