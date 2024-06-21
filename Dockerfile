#FROM microsoft/dotnet-framework:4.7.2-sdk-windowsservercore-1803 AS build
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 AS build

WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY MERCY.Web.FrontEnd/*.csproj ./MERCY.Web.FrontEnd/
COPY Mercy.Cmd.Scheduler/*.csproj ./Mercy.Cmd.Scheduler/
COPY Mercy.Cmd.LECO/*.csproj ./Mercy.Cmd.LECO/
COPY MERCY.Web.FrontEnd/*.config ./MERCY.Web.FrontEnd/
RUN nuget restore

# copy everything else and build app
COPY MERCY.Web.FrontEnd/. ./MERCY.Web.FrontEnd/
WORKDIR /app/MERCY.Web.FrontEnd
RUN msbuild /p:Configuration=Release
#RUN msbuild MERCY.Web.FrontEnd.csproj /p:DeployOnBuild=true /p:PublishProfile=FolderProfile


#FROM microsoft/aspnet:4.7.2-windowsservercore-1803 AS runtime
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8 AS runtime

WORKDIR /inetpub/wwwroot
COPY --from=build /app/MERCY.Web.FrontEnd/. ./