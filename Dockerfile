FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base


WORKDIR /app


FROM ghcr.io/lotfiaghel/base8:dev AS build


COPY /TestHelper2.sln ./
COPY /Basics/ExtraBaseQueryTool/ExtraBaseQueryTool.csproj ./Basics/ExtraBaseQueryTool/
COPY /Basics/BaseModels/BaseModels.csproj ./Basics/BaseModels/
COPY /Basics/BasicAttributes/BasicAttributes.csproj ./Basics/BasicAttributes/
COPY /Basics/BaseDataAnottaions/BaseDataAnottaions.csproj ./Basics/BaseDataAnottaions/
COPY /Constants/Constants.csproj ./Constants/
COPY /Models/Models.csproj ./Models/
COPY /Data/Data.csproj ./Data/
COPY /AdminServer/AdminServer.csproj ./AdminServer/
COPY /AdminClient/AdminClient.csproj ./AdminClient/
COPY /WebApplication/WebApplication.csproj ./WebApplication/
COPY /Repositoris/Repositoris.csproj ./Repositoris/
COPY /old_testhelper/Data/old_Data.csproj /old_testhelper/Data/
COPY /old_testhelper/Models/old_Models.csproj /old_testhelper/Models/
COPY /old_testhelper/ViewGeneratorBase/ViewGeneratorBase.csproj /old_testhelper/ViewGeneratorBase/




RUN echo "restore depedencis"
RUN dotnet restore /Models/Models.csproj
RUN dotnet restore /Data/Data.csproj
RUN dotnet restore /WebApplication/WebApplication.csproj
RUN dotnet restore /AdminServer/AdminServer.csproj
RUN dotnet restore /AdminClient/AdminClient.csproj

RUN echo "resore end"

