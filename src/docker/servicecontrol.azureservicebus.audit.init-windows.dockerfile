FROM mcr.microsoft.com/windows/servercore:ltsc2016

# Taken from https://github.com/microsoft/dotnet-framework-docker/blob/master/src/runtime/4.7.2/windowsservercore-ltsc2016/Dockerfile
RUN \
    # Install .NET 4.7.2
    powershell -Command \
        $ProgressPreference = 'SilentlyContinue'; \
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; \
        Invoke-WebRequest \
            -UseBasicParsing \
            -Uri "https://download.microsoft.com/download/6/E/4/6E48E8AB-DC00-419E-9704-06DD46E5F81D/NDP472-KB4054530-x86-x64-AllOS-ENU.exe" \
            -OutFile dotnet-framework-installer.exe \
    && start /w .\dotnet-framework-installer.exe /q \
    && del .\dotnet-framework-installer.exe \
    && powershell Remove-Item -Force -Recurse ${Env:TEMP}\*

ENV "SERVICECONTROL_RUNNING_IN_DOCKER"="true"

WORKDIR /servicecontrol.audit

ADD /ServiceControl.Transports.ASBS/bin/Release/net472 .
ADD /ServiceControl.Audit/bin/Release/net472 .

ENV "ServiceControl.Audit/TransportType"="ServiceControl.Transports.ASBS.ASBSTransportCustomization, ServiceControl.Transports.ASBS"
ENV "ServiceControl.Audit/Hostname"="*"

ENV "ServiceControl.Audit/DBPath"="C:\\Data\\DB\\"
ENV "ServiceControl.Audit/LogPath"="C:\\Data\\Logs\\"

# Defaults
ENV "ServiceControl.Audit/ForwardAuditMessages"="False"
ENV "ServiceControl.Audit/AuditRetentionPeriod"="365"

ENTRYPOINT ["ServiceControl.Audit.exe", "--portable", "--setup"]