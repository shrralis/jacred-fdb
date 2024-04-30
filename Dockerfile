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
WORKDIR $JACRED_HOME

COPY --from=builder ./opt/src .
COPY ./start.sh .

RUN apk add --no-cache --update aspnetcore6-runtime

EXPOSE 9117
CMD ["./start.sh"]
### BUILD MAIN IMAGE end ###
