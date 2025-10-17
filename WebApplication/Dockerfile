FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base


WORKDIR /app


FROM ghcr.io/lotfiaghel/tradebase8:dev AS build

# "I dont now way wasm tools needed"

#RUN apt-get install htop -y dosent work :()



COPY /DICOMServer.sln ./

COPY /Basics/ExtraBaseQueryTool/ExtraBaseQueryTool.csproj ./Basics/ExtraBaseQueryTool/
COPY /Basics/BaseModels/BaseModels.csproj ./Basics/BaseModels/
COPY /Basics/BasicAttributes/BasicAttributes.csproj ./Basics/BasicAttributes/
COPY /Basics/BaseDataAnottaions/BaseDataAnottaions.csproj ./Basics/BaseDataAnottaions/
COPY /Constants/Constants.csproj ./Constants/
COPY /Models/Models.csproj ./Models/
COPY /Data/Data.csproj ./Data/
COPY /WebApplication/WebApplication.csproj ./WebApplication/
COPY /google-api-dotnet-client ./google-api-dotnet-client




RUN dotnet restore /WebApplication/WebApplication.csproj

COPY /EnglishToefle/EnglishToefl.csproj ./EnglishToefle/
RUN dotnet restore /WebApplication/WebApplication.csproj

RUN echo "restore depedencis"


COPY /Basics/ExtraBaseQueryTool ./Basics/ExtraBaseQueryTool
COPY /Basics/BaseModels ./Basics/BaseModels/
COPY /Basics/BasicAttributes ./Basics/BasicAttributes
COPY /Basics/BaseDataAnottaions ./Basics/BaseDataAnottaions
COPY /Constants ./Constants
COPY /Models ./Models
COPY /Data ./Data
COPY /Repositoris /Repositoris
COPY /WebApplication ./WebApplication

WORKDIR /WebApplication
RUN echo `pwd`
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app
ARG GIT_COMMIT=GIT_COMMIT
RUN echo $GIT_COMMIT  > /app/version.txt
FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WebApplication.dll"]