#FROM microsoft/dotnet-framework:4.7.2-sdk-windowsservercore-1803 AS build
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 AS build

WORKDIR /app

# copy csproj and restore as distinct layers
# COPY *.sln .
COPY . .
# COPY MERCY.Data/*.csproj ./MERCY.Data/
# COPY MERCY.Web.BackEnd/*.config ./MERCY.Web.BackEnd/
RUN nuget restore

# copy everything else and build app
# COPY MERCY.Web.BackEnd/. ./MERCY.Web.BackEnd/
WORKDIR /app/MERCY.Web.BackEnd
RUN msbuild MERCY.Web.BackEnd.csproj /p:DeployOnBuild=true /p:Configuration=Release /P:PublishProfile=FolderProfile.pubxml
# RUN msbuild /p:Configuration=Release
#RUN msbuild MERCY.Web.FrontEnd.csproj /p:DeployOnBuild=true /p:PublishProfile=FolderProfile


#FROM microsoft/aspnet:4.7.2-windowsservercore-1803 AS runtime
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8 AS runtime

WORKDIR /inetpub/wwwroot
COPY --from=build /app/MERCY.Web.BackEnd/. ./