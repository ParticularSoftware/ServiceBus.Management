﻿namespace ServiceBus.Management.Infrastructure.Settings
{
    using System;
    using System.IO;
    using System.Linq;
    using NServiceBus;
    using NServiceBus.Logging;

    public class Settings
    {
        public static int Port
        {
            get
            {
                if (port == 0)
                {
                    port = SettingsReader<int>.Read("Port", 33333);
                }

                return port;
            }
        }

        public static string Schema
        {
            get
            {
                if (schema == null)
                {
                    schema = SettingsReader<string>.Read("Schema", "http");
                }

                return schema;
            }
        }

        public static string Hostname
        {
            get
            {
                if (hostname == null)
                {
                    hostname = SettingsReader<string>.Read("Hostname", "localhost");
                }

                return hostname;
            }
        }

        public static string VirtualDirectory
        {
            get
            {
                if (virtualDirectory == null)
                {
                    virtualDirectory = SettingsReader<string>.Read("VirtualDirectory", String.Empty);
                }

                return virtualDirectory;
            }
        }

        public static string ApiUrl
        {
            get
            {
                var suffix = VirtualDirectory;

                if (!string.IsNullOrEmpty(suffix))
                {
                    suffix += "/";
                }

                suffix += "api/";

                return string.Format("{0}://{1}:{2}/{3}", Schema, Hostname, Port, suffix);
            }
        }

        public static string LogPath
        {
            get
            {
                if (logPath == null)
                {
                    logPath = SettingsReader<string>.Read("LogPath", "${specialfolder:folder=ApplicationData}/Particular/ServiceControl/logs");
                }

                return logPath;
            }
        }

        public static string StorageUrl
        {
            get
            {
                var suffix = VirtualDirectory;

                if (!string.IsNullOrEmpty(suffix))
                {
                    suffix += "/";
                }

                suffix += "storage/";

                return string.Format("{0}://{1}:{2}/{3}", Schema, Hostname, Port, suffix);
            }
        }

        public static Address AuditQueue
        {
            get
            {
                if (auditQueue != null)
                {
                    return auditQueue;
                }

                var value = SettingsReader<string>.Read("ServiceBus", "AuditQueue", null);

                if (value != null)
                {
                    auditQueue = Address.Parse(value);
                }
                else
                {
                    Logger.Warn(
                        "No settings found for audit queue to import, if this is not intentional please set add ServiceBus/AuditQueue to your appSettings");

                    auditQueue = Address.Undefined;
                }

                return auditQueue;
            }
        }

        public static Address ErrorQueue
        {
            get
            {
                if (errorQueue != null)
                {
                    return errorQueue;
                }

                var value = SettingsReader<string>.Read("ServiceBus", "ErrorQueue", null);

                if (value != null)
                {
                    errorQueue = Address.Parse(value);
                }
                else
                {
                    Logger.Warn(
                        "No settings found for error queue to import, if this is not intentional please set add ServiceBus/ErrorQueue to your appSettings");

                    errorQueue = Address.Undefined;
                }

                return errorQueue;
            }
        }

        public static Address ErrorLogQueue
        {
            get
            {
                if (errorLogQueue == null)
                {
                    var value = SettingsReader<string>.Read("ServiceBus", "ErrorLogQueue", null);

                    if (value != null)
                    {
                        errorLogQueue = Address.Parse(value);
                    }
                    else
                    {
                        Logger.Info("No settings found for error log queue to import, default name will be used");

                        errorLogQueue = ErrorQueue.SubScope("log");
                    }
                }

                return errorLogQueue;
            }
        }

        public static Address AuditLogQueue
        {
            get
            {
                if (auditLogQueue == null)
                {
                    var value = SettingsReader<string>.Read("ServiceBus", "AuditLogQueue", null);

                    if (value != null)
                    {
                        auditLogQueue = Address.Parse(value);
                    }
                    else
                    {
                        Logger.Info("No settings found for audit log queue to import, default name will be used");

                        auditLogQueue = AuditQueue.SubScope("log");
                    }
                }

                return auditLogQueue;
            }
        }

        public static bool ForwardAuditMessages
        {
            get
            {
                return forwardAuditMessages;
            }
        }

        public static string DbPath
        {
            get
            {
                if (dbPath == null)
                {
                    var host = Hostname;
                    if (host == "*")
                    {
                        host = "%";
                    }
                    var dbFolder = String.Format("{0}-{1}", host, Port);

                    if (!string.IsNullOrEmpty(VirtualDirectory))
                    {
                        dbFolder += String.Format("-{0}", SanitiseFolderName(VirtualDirectory));
                    }

                    var defaultPath =
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                            "Particular", "ServiceControl", dbFolder);

                    dbPath = SettingsReader<string>.Read("DbPath", defaultPath);
                }

                return dbPath;
            }
        }

        public static bool CreateIndexSync {
            get
            {
                return SettingsReader<bool>.Read("CreateIndexSync");       
            } 
        }

        static string SanitiseFolderName(string folderName)
        {
            return Path.GetInvalidPathChars().Aggregate(folderName, (current, c) => current.Replace(c, '-'));
        }

        static int port;
        static string hostname, schema, logPath;
        static string virtualDirectory;
        static string dbPath;
        static readonly bool forwardAuditMessages = SettingsReader<bool>.Read("ForwardAuditMessages");
        static Address auditQueue;
        static Address errorLogQueue, auditLogQueue;
        static Address errorQueue;
        static readonly ILog Logger = LogManager.GetLogger(typeof(Settings));
    }
}