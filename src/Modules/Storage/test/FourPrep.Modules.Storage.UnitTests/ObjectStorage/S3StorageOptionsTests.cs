namespace FourPrep.Modules.Storage.UnitTests.ObjectStorage;

using FourPrep.Modules.Storage.Infrastructure.ObjectStorage;

public class S3StorageOptionsTests
{
    [Fact]
    public void KeepsDocumentsPrivate_AcceptsASeparatePublicBucket()
    {
        Assert.True(S3StorageOptions.KeepsDocumentsPrivate(Options()));
    }

    [Theory]
    [InlineData("fourprep-documents")]
    [InlineData(" FourPrep-Documents ")]
    public void KeepsDocumentsPrivate_RefusesTheDocumentsBucketAsThePublicOne(string publicBucket)
    {
        Assert.False(S3StorageOptions.KeepsDocumentsPrivate(Options(publicBucket: publicBucket)));
    }

    [Theory]
    [InlineData("http://localhost:8333/fourprep-documents")]
    [InlineData("https://fourprep-documents.r2.example.com")]
    public void KeepsDocumentsPrivate_RefusesAPublicAddressOfTheDocumentsBucket(string publicBaseUrl)
    {
        Assert.False(S3StorageOptions.KeepsDocumentsPrivate(Options(publicBaseUrl: publicBaseUrl)));
    }

    [Theory]
    [InlineData("media.example.com")]
    [InlineData("ftp://media.example.com")]
    public void KeepsDocumentsPrivate_RefusesAnAddressThatIsNotAbsoluteHttp(string publicBaseUrl)
    {
        Assert.False(S3StorageOptions.KeepsDocumentsPrivate(Options(publicBaseUrl: publicBaseUrl)));
    }

    private static S3StorageOptions Options(
        string publicBucket = "fourprep-public",
        string publicBaseUrl = "http://localhost:8333/fourprep-public") =>
        new()
        {
            ServiceUrl = "http://localhost:8333",
            Region = "us-east-1",
            AccessKey = "key",
            SecretKey = "secret",
            DocumentsBucket = "fourprep-documents",
            PublicBucket = publicBucket,
            PublicBaseUrl = publicBaseUrl
        };
}
