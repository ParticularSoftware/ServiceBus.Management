FROM mcr.microsoft.com/dotnet/framework/runtime:4.7.2-windowsservercore-ltsc2016

WORKDIR /servicecontrol.monitoring

ADD /ServiceControl.Transports.SqlServer/bin/Release/net472 .
ADD /ServiceControl.Monitoring/bin/Release/net472 .

ENV "SERVICECONTROL_RUNNING_IN_DOCKER"="true"

ENV "Monitoring/TransportType"="ServiceControl.Transports.SqlServer.SqlServerTransportCustomization, ServiceControl.Transports.SqlServer"
ENV "Monitoring/HttpHostName"="*"
ENV "Monitoring/HttpPort"="33633"

ENV "Monitoring/LogPath"="C:\\Data\\Logs\\"

EXPOSE 33633

VOLUME [ "C:/Data" ]

ENTRYPOINT ["ServiceControl.Monitoring.exe", "--portable"]