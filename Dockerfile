# ### BUILD MAIN IMAGE START ###
FROM ubuntu

COPY ./docker-install.sh /install.sh
RUN /bin/bash /install.sh

WORKDIR /home/jacred
EXPOSE 9117
VOLUME ["/home/jacred"]
ENTRYPOINT [".dotnet/dotnet", "JacRed.dll"]
### BUILD MAIN IMAGE end ###
