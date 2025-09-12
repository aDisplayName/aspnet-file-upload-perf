using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FileUploadPerformance.ApiService;

/// <summary>
/// Controller for access the source input data.
/// </summary>
[ApiController]
[Route("api/v1/files")]
public class FileUploadController: ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllFilesAsync(IFileStorageService fileStorage,
        CancellationToken cancellationToken)
    {
            return new OkObjectResult(await fileStorage.GetFileListAsync(cancellationToken));
    }

    [HttpPut]
    [UploadSizeLimitFilter()]
    // Disable the form value model binding, so that we can still use query string parameter, as well as the cancellation token, without pre-read the content of the file stream.
    // Otherwise, we have to use argument-less signature to support stream (where the form data is not pre-read),
    // which will leads to the lack of cancellation token and other query based information to be passed on.
    // For argument-less, please refer to https://github.com/dotnet/AspNetCore.Docs/tree/main/aspnetcore/mvc/models/file-uploads/samples/5.x/LargeFilesSample
    [DisableFormValueModelBinding]
    public async Task<IActionResult> UploadFileAsync(IFileStorageService fileStorage,
        CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue("targetFileName", out var targetFileNamePathValue))
        {
            return new BadRequestObjectResult(new ErrorResultData("targetFileName header is missing."));
        }

        var targetFileNamePath = targetFileNamePathValue.FirstOrDefault() ?? string.Empty;
        var needCleanUp = false;
        try
        {
            needCleanUp = true;

            // raw file name will be used to do the additional operation.
            var filenameWritten=await fileStorage.PerformSingleFileUploadInternalAsync(HttpContext.Request, targetFileNamePath, cancellationToken)
                .ConfigureAwait(false);
            needCleanUp = false;
            return Ok(filenameWritten);
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(new ErrorResultData(e.Message))
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
        finally
        {
            if (needCleanUp)
            {
                try
                {
                    await fileStorage.RemoveFileAsync(targetFileNamePath);
                }
                catch
                {
                    // Ignore
                }
            }
        }
    }
}