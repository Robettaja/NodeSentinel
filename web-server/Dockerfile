FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y nginx && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
COPY nginx.conf /etc/nginx/nginx.conf

RUN echo '#!/bin/sh\nnginx\ndotnet /app/web-server.dll' > /start.sh && chmod +x /start.sh

EXPOSE 80
ENTRYPOINT ["/start.sh"]
