FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base


WORKDIR /app


FROM ghcr.io/lotfiaghel/tradebase8:dev AS build




COPY /DICOMServer.sln ./

COPY /Basics/ExtraBaseQueryTool/ExtraBaseQueryTool.csproj ./Basics/ExtraBaseQueryTool/
COPY /Basics/BaseModels/BaseModels.csproj ./Basics/BaseModels/
COPY /Basics/AdminClientViewModels/AdminClientViewModels.csproj ./Basics/AdminClientViewModels/
COPY /Basics/AdminBaseComponenets/AdminBaseComponenets.csproj ./Basics/AdminBaseComponenets/
COPY /Basics/BasicAttributes/BasicAttributes.csproj ./Basics/BasicAttributes/
COPY /Basics/BaseDataAnottaions/BaseDataAnottaions.csproj ./Basics/BaseDataAnottaions/
COPY /Basics/Client/Client.csproj ./Basics/Client/
COPY /Models/Models.csproj ./Models/
COPY /AdminModels/AdminModels.csproj ./AdminModels/
COPY /old_testhelper/ViewGeneratorBase/ViewGeneratorBase.csproj /old_testhelper/ViewGeneratorBase/




COPY /AdminClient/AdminClient.csproj ./AdminClient/

RUN dotnet restore "/AdminClient/AdminClient.csproj"


RUN echo "salam"




COPY /Basics ./Basics

COPY /Constants ./Constants
COPY /Models ./Models
COPY /AdminModels ./AdminModels
COPY /AdminClient ./AdminClient
COPY /old_testhelper/Models /old_testhelper/Models
COPY /old_testhelper/ViewGeneratorBase /old_testhelper/ViewGeneratorBase

#




WORKDIR /AdminClient

RUN echo `pwd`
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

#FROM nginx AS final
FROM nginx:latest AS final


RUN sed -i '1idaemon off;' /etc/nginx/nginx.conf
COPY /AdminClient/nginx.conf /etc/nginx/conf.d/default.conf
WORKDIR /app
#COPY --from=publish /app/wwwroot /usr/share/nginx/html
COPY --from=publish /app/wwwroot /app


EXPOSE 80
EXPOSE 443

CMD ["nginx"]


