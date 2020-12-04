FROM mcr.microsoft.com/windows/servercore:ltsc2016

WORKDIR /servicecontrol.audit

ADD /ServiceControl.Transports.ASQ/bin/Release/net462 .
ADD /ServiceControl.Audit/bin/Release/net462 .

ENV "ServiceControl.Audit/TransportType"="ServiceControl.Transports.ASQ.ASQTransportCustomization, ServiceControl.Transports.ASQ"
ENV "ServiceControl.Audit/Hostname"="*"

ENV "ServiceControl.Audit/DBPath"="C:\\Data\\"
ENV "ServiceControl.Audit/LogPath"="C:\\Logs\\"

ENTRYPOINT ["ServiceControl.Audit.exe", "--portable", "--setup"]