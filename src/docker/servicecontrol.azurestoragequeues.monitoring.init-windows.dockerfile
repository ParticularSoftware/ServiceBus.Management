FROM mcr.microsoft.com/windows/servercore:ltsc2016

WORKDIR /servicecontrol.monitoring

ADD /ServiceControl.Transports.ASQ/bin/Release/net462 .
ADD /ServiceControl.Monitoring/bin/Release/net462 .

ENV "Monitoring/TransportType"="ServiceControl.Transports.ASQ.ASQTransportCustomization, ServiceControl.Transports.ASQ"
ENV "Monitoring/HttpHostName"="*"

ENTRYPOINT ["ServiceControl.Monitoring.exe", "--portable", "--setup"]