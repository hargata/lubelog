FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

COPY . ./
ARG TARGETARCH
RUN dotnet restore CarCareTracker.csproj -a $TARGETARCH
RUN dotnet publish CarCareTracker.csproj -a $TARGETARCH -c Release -o out
RUN dotnet publish HealthCheck/HealthCheck.csproj -a $TARGETARCH -c Release -o out/healthcheck

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
  CMD /App/healthcheck/HealthCheck || exit 1
CMD ["./CarCareTracker"]
