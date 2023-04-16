FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/nightly/sdk:8.0-preview-alpine AS build
ARG TARGETARCH

WORKDIR /src
COPY ["CloudFlareZeroTrustOperator/CloudFlareZeroTrustOperator.csproj", "CloudFlareZeroTrustOperator/"]
RUN dotnet restore "CloudFlareZeroTrustOperator/CloudFlareZeroTrustOperator.csproj" -a $TARGETARCH
COPY . .
WORKDIR "/src/CloudFlareZeroTrustOperator"
RUN dotnet publish "CloudFlareZeroTrustOperator.csproj" -c Release -o /app/publish /p:UseAppHost=false --self-contained false -a $TARGETARCH 

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as base

RUN addgroup k8s-operator && useradd -G k8s-operator operator-user

WORKDIR /operator
COPY --from=build /app/publish ./
RUN chown operator-user:k8s-operator -R .
USER operator-user
ENTRYPOINT [ "dotnet", "CloudFlareZeroTrustOperator.dll" ]