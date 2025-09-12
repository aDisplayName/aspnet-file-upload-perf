namespace FileUploadPerformance.ApiService;

public class SystemSettings
{
    public long MaxFileUploadLimit { get; set; } = 100_000_000L;

    public string? RootFolder { get; set; }
}