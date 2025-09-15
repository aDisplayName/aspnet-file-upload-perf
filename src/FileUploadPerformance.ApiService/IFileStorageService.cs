namespace FileUploadPerformance.ApiService;

public interface IFileStorageService
{
    Task<IEnumerable<string>> GetFileListAsync(CancellationToken cancellationToken);

    Task<string> PerformSingleFileUploadInternalAsync(HttpRequest request, string targetFilePath, CancellationToken ctx = default);
    Task RemoveFileAsync(string filename);
}