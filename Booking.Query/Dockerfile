FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App
COPY . ./

RUN dotnet restore
RUN dotnet publish -c Release -o out -r linux-x64

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .

RUN apt update
RUN apt upgrade
RUN apt -y install curl

EXPOSE 80
ENTRYPOINT ["dotnet","Booking.Query.dll"]