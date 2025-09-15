FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS dotnet-builder
ARG APP_VERSION=0.0.0.0
COPY src src

RUN dotnet publish ./src/FileUploadPerformance.ApiService -c Release -p:Version=${APP_VERSION} --output /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled-composite

WORKDIR /app
COPY --from=dotnet-builder /app /app

CMD ["FileUploadPerformance.ApiService.dll"]