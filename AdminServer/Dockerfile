FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base


WORKDIR /app


FROM ghcr.io/lotfiaghel/tradebase8:dev AS build

# "I dont now way wasm tools needed"





COPY /TestHelper2.sln ./

COPY /Basics/ExtraBaseQueryTool/ExtraBaseQueryTool.csproj ./Basics/ExtraBaseQueryTool/
COPY /Basics/BaseModels/BaseModels.csproj ./Basics/BaseModels/
COPY /Basics/BasicAttributes/BasicAttributes.csproj ./Basics/BasicAttributes/
COPY /Basics/BaseDataAnottaions/BaseDataAnottaions.csproj ./Basics/BaseDataAnottaions/
COPY /Constants/Constants.csproj ./Constants/
COPY /Models/Models.csproj ./Models/
COPY /AdminModels/AdminModels.csproj ./AdminModels/
COPY /Data/Data.csproj ./Data/
COPY /google-api-dotnet-client ./google-api-dotnet-client
COPY /AdminServer/AdminServer.csproj ./AdminServer/
COPY /WebApplication/WebApplication.csproj ./WebApplication/
COPY /old_testhelper/Data/old_Data.csproj ./old_testhelper/Data/
COPY /old_testhelper/Models/old_Models.csproj ./old_testhelper/Models/
COPY /old_testhelper/ViewGeneratorBase/ViewGeneratorBase.csproj ./old_testhelper/ViewGeneratorBase/
COPY /Repositoris/Repositoris.csproj ./Repositoris/

COPY /NewRepositoris/NewRepositoris.csproj ./NewRepositoris/
COPY /Migrator/Migrator.csproj ./Migrator/
COPY /EnglishToefle/EnglishToefl.csproj ./EnglishToefle/
COPY /TestHelperAI/TestHelperAI.csproj ./TestHelperAI/

RUN dotnet restore /AdminServer/AdminServer.csproj

RUN echo "restore depedencis"


COPY /Basics/ExtraBaseQueryTool ./Basics/ExtraBaseQueryTool
COPY /Basics/BaseModels ./Basics/BaseModels/
COPY /Basics/BasicAttributes ./Basics/BasicAttributes
COPY /Basics/BaseDataAnottaions ./Basics/BaseDataAnottaions
COPY /Constants ./Constants
COPY /Models ./Models
COPY /AdminModels ./AdminModels
COPY /Data ./Data
COPY /old_testhelper/Data ./old_testhelper/Data
COPY /old_testhelper/Models ./old_testhelper/Models
COPY /old_testhelper/ViewGeneratorBase ./old_testhelper/ViewGeneratorBase
COPY /Repositoris ./Repositoris
COPY /WebApplication ./WebApplication
COPY /AdminServer ./AdminServer


COPY /NewRepositoris ./NewRepositoris/
COPY /Migrator ./Migrator/
COPY /EnglishToefle ./EnglishToefle/
COPY /TestHelperAI ./TestHelperAI/
COPY /OpenAiWrapper ./OpenAiWrapper/

WORKDIR /AdminServer
RUN echo `pwd`
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

ARG GIT_COMMIT=GIT_COMMIT
RUN echo $GIT_COMMIT  > /app/version.txt

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AdminServer.dll"]