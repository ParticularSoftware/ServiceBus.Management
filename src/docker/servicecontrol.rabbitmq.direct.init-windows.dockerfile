FROM mcr.microsoft.com/windows/servercore:ltsc2016

WORKDIR /servicecontrol

ADD /ServiceControl.Transports.RabbitMQ/bin/Release/net472 .
ADD /ServiceControl/bin/Release/net472 .

ENV "SERVICECONTROL_RUNNING_IN_DOCKER"="true"

ENV "ServiceControl/TransportType"="ServiceControl.Transports.RabbitMQ.RabbitMQDirectRoutingTransportCustomization, ServiceControl.Transports.RabbitMQ"
ENV "ServiceControl/Hostname"="*"

ENV "ServiceControl/DBPath"="C:\\Data\\DB\\"
ENV "ServiceControl/LogPath"="C:\\Data\\Logs\\"

# Defaults
ENV "ServiceControl/ForwardErrorMessages"="False"
ENV "ServiceControl/ErrorRetentionPeriod"="15"

ENTRYPOINT ["ServiceControl.exe", "--portable", "--setup"]