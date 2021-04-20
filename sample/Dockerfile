FROM mcr.microsoft.com/dotnet/sdk:5.0.202-alpine3.13 AS builder
WORKDIR /build
ADD Scaleway.Functions.Sample.csproj .
RUN dotnet restore
ADD . .
RUN dotnet publish -c Release

FROM mcr.microsoft.com/dotnet/aspnet:5.0.5-alpine3.13
WORKDIR /app
COPY --from=builder /build/bin/Release/net5.0/publish .
CMD [ "dotnet", "Scaleway.Functions.Sample.dll" ]
