FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "src/ReExamManagementSystem.Web/ReExamManagementSystem.Web.csproj"
RUN dotnet publish "src/ReExamManagementSystem.Web/ReExamManagementSystem.Web.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "ReExamManagementSystem.Web.dll"]
