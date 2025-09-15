using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace FileUploadPerformance.ApiService;

[AttributeUsage(AttributeTargets.Method)]
public class UploadSizeLimitFilter : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var systemSettingOption = context.HttpContext.RequestServices.GetService<IOptions<SystemSettings>>();

        if (systemSettingOption is null)
            return;

        if (systemSettingOption.Value.MaxFileUploadLimit <= 0)
        {
            return;
        }

        if (context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>() is { } maxRequestBodySizeFeature)
        {
            maxRequestBodySizeFeature.MaxRequestBodySize = systemSettingOption.Value.MaxFileUploadLimit;
        }
    }
}