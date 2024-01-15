FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

COPY . ./
ARG TARGETARCH
RUN dotnet restore -a $TARGETARCH
RUN dotnet publish -a $TARGETARCH -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .
EXPOSE 8080
CMD ["./CarCareTracker"]
