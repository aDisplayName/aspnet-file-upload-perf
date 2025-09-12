using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace FileUploadPerformance.ApiService;

public class FileStorageService(IOptions<SystemSettings> settings) : IFileStorageService
{
    public async Task<IEnumerable<string>> GetFileListAsync(CancellationToken cancellationToken)
    {
        return Directory.GetFiles(settings.Value.RootFolder);
    }

    public async Task<string> PerformSingleFileUploadInternalAsync(HttpRequest request, string targetFilePath,
        CancellationToken ctx)
    {
        // validation of Content-Type
        // 1. first, it must be a form-data request
        // 2. a boundary should be found in the Content-Type
        if (!request.HasFormContentType ||
            !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader) ||
            string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
        {
            throw new UnsupportedContentTypeException("The request does not contain a valid multipart form-data");
        }

        // TODO: double check if https://github.com/dotnet/aspnetcore/issues/41237 has been fixed in .net 7
        var boundaryValue = mediaTypeHeader.Boundary.Value;


        var reader = new MultipartReader(boundaryValue, request.Body);

        var section = await reader.ReadNextSectionAsync(ctx);


        // This sample try to get the first file from request and save it
        // Make changes according to your needs in actual use

        while (section != null)
        {
            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                out var contentDisposition);


            if (hasContentDispositionHeader)
            {
                if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))

                {
                    // Don't trust any file name, file extension, and file data from the request unless you trust them completely
                    // Otherwise, it is very likely to cause problems such as virus uploading, disk filling, etc
                    // In short, it is necessary to restrict and verify the upload
                    // Here, we just use the temporary folder and a random file name
                    var originalFilename = contentDisposition!.FileName.Value;
                    await using var targetStream = new FileStream(Path.Join(settings.Value.RootFolder, targetFilePath),
                        FileMode.CreateNew, FileAccess.Write, FileShare.None);
                    long srcLen = 0;
                    try
                    {
                        srcLen = section.Body.Length;
                    }
                    catch (NotSupportedException)
                    {
                        // Ignore if src length was not known.
                    }

                    if (srcLen > 0)
                    {
                        try
                        {
                            // Pre-claim the storage to detect out of storage error as soon as possible.
                            targetStream.SetLength(srcLen);
                            targetStream.Seek(0, SeekOrigin.Begin);
                        }
                        catch (IOException)
                        {
                            throw;
                        }
                        catch
                        {
                            // Ignore if otherwise.
                        }
                    }

                    await section.Body.CopyToAsync(targetStream, ctx);
                    return originalFilename;
                }
            }

            section = await reader.ReadNextSectionAsync(ctx);
        }

        // If the code runs to this location, it means that no files have been saved
        throw new UnsupportedContentTypeException("No valid file data found");

    }

    public Task RemoveFileAsync(string filename)
    {
        File.Delete(Path.Join(settings.Value.RootFolder, filename));
        return Task.CompletedTask;
    }
}