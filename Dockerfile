### BUILD JACRED MULTIARCH START ###
FROM --platform=$BUILDPLATFORM alpine AS builder

WORKDIR /opt/src

ARG TARGETARCH

# Step for multiarch build with docker buildx
ENV GOARCH=$TARGETARCH

# Get and unpack JacRed
RUN apk add --update bash wget unzip
RUN wget https://github.com/immisterio/jacred-fdb/releases/latest/download/publish.zip
RUN unzip -o publish.zip
RUN rm -f publish.zip
### BUILD JACRED MULTIARCH END ###


# ### BUILD MAIN IMAGE START ###
FROM alpine

ENV JACRED_HOME=/home/jacred

COPY --from=builder ./opt/src $JACRED_HOME/

RUN apk add --no-cache --update aspnetcore6-runtime

WORKDIR $JACRED_HOME

EXPOSE 9117
CMD ["dotnet", "JacRed.dll"]
### BUILD MAIN IMAGE end ###
