FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY ./ ./
RUN dotnet publish -c Debug -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0.0-bionic 

RUN apt-get update 
# Instal vim
RUN apt-get install vim -y
# Instal libreoffice
ENV DEBIAN_FRONTEND noninteractive
RUN mkdir -p /usr/share/man/man1mkdir -p /usr/share/man/man1
RUN apt-get -y -q install software-properties-common && yes | add-apt-repository ppa:libreoffice/ppa && apt-get update
RUN apt-get -y -q install gdb libreoffice libreoffice-writer ure libreoffice-java-common libreoffice-core libreoffice-common openjdk-8-jre fonts-opensymbol hyphen-fr hyphen-de hyphen-en-us hyphen-it hyphen-ru fonts-dejavu fonts-dejavu-core fonts-dejavu-extra fonts-droid-fallback fonts-dustin fonts-f500 fonts-fanwood fonts-freefont-ttf fonts-liberation fonts-lmodern fonts-lyx fonts-sil-gentium fonts-texgyre fonts-tlwg-purisa tzdata
RUN apt-get -y -q remove libreoffice-gnome
RUN adduser --home=/opt/libreoffice --disabled-password --gecos "" --shell=/bin/bash libreoffice
RUN apt-get autoclean
WORKDIR /app
RUN mkdir /code
COPY --from=build-env /app /code
RUN cp -r /code/out/* /app/
RUN ln -fs /usr/share/zoneinfo/Europe/Kiev /etc/localtime && dpkg-reconfigure -f noninteractive tzdata
EXPOSE 80
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "convertservice.dll"]

