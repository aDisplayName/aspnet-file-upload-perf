var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.FileUploadPerformance_ApiService>("apiservice")
    .WithUrlForEndpoint("https", url =>
    {
        url.DisplayText = "Scalar (HTTPS)";
        url.Url = "/dev";
    });

builder.Build().Run();
