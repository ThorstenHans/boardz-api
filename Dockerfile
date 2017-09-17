FROM microsoft/dotnet:2.0.0-sdk as build-env
LABEL maintainer="Thorsten Hans <thorsten.hans@thinktecture.com>"

WORKDIR /app
COPY ./code/*.csproj /app

RUN dotnet restore

COPY ./code/ /app

RUN dotnet publish BoardZ.API.csproj -c Release -o /app/out

FROM microsoft/aspnetcore:2.0
WORKDIR /app
EXPOSE 80
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "BoardZ.API.dll"]

