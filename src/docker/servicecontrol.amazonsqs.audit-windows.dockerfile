FROM mcr.microsoft.com/windows/servercore:ltsc2016

WORKDIR /servicecontrol.audit

ADD /ServiceControl.Transports.SQS/bin/Release/net462 .
ADD /ServiceControl.Audit/bin/Release/net462 .

ENV "ServiceControl.Audit/TransportType"="ServiceControl.Transports.SQS.SQSTransportCustomization, ServiceControl.Transports.SQS"
ENV "ServiceControl.Audit/Hostname"="*"

ENV "ServiceControl.Audit/DBPath"="C:\\Data\\"
ENV "ServiceControl.Audit/LogPath"="C:\\Logs\\"

EXPOSE 44444
EXPOSE 44445

ENTRYPOINT ["ServiceControl.Audit.exe", "--portable"]