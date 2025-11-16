FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG TARGETARCH
ARG BUILDPLATFORM
WORKDIR /operator


RUN echo "TARGETARCH=$TARGETARCH"
RUN echo "BUILDPLATFORM=$BUILDPLATFORM"
COPY ./ ./


RUN dotnet publish "Uptime-kuma-operator.csproj" -c Release -o out \
    --self-contained true \
    /p:PublishSingleFile=true \
    /p:PublishTrimmed=true \
    /p:EnableCompressionInSingleFile=true \
    /p:TrimMode=partial \
    -a $TARGETARCH
RUN ls out

# The runner for the application
FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-alpine AS final
ARG TARGETARCH
ARG BUILDPLATFORM
WORKDIR /operator
COPY --from=build /operator/out/ ./
RUN ls /operator
RUN chmod -R 777 /operator

ENTRYPOINT ["/operator/Uptime-kuma-operator" ]
