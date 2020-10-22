﻿using Sulu.Dto;
using Sulu.Platform;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Sulu.Launch
{
    class Application : IApplication
    {
        Launch.Options Options { get; }

        ISuluConfiguration Config { get; }

        IOsServices OsServices { get; }

        public Application(Launch.Options options, IOsServices osServices, ISuluConfiguration config)
        {
            this.Options = options;
            Config = config;
            OsServices = osServices;
        }

        public int Run()
        {
            Serilog.Log.Debug($"Dispatching URL: {Options.Url}");

            try
            {
                using var handler = Config.GetProtocolHandler(Options.Url);
                if(handler == null)
                {
                    // We are the registered application, but we can't parse the config
                    // or nothing is configured, show an error somehow
                    OsServices.OpenText($"Sulu configuration does not provide a method to handle the URL: {Options.Url}");
                    return 1;
                }
                handler.Run();
                return 0;
            }
            catch(Exception ex)
            {
                HandleLaunchError(ex, Options.Url);
            }
            return 1;
        }

        private void HandleLaunchError(Exception ex, string url)
        {
            var application = GetApplicationForProtocol(Config.GetConfiguration(), GetProtocol(url));
            var suluJsonPath = Path.Combine(Constants.GetBinaryDir(), "sulu.json");
            var msg =
$@"[SULU]: Failed to launch registered URL handler.
  URL:           {url}
  Error:         {ex.Message}

  ApplicationId: {application?.Id ?? "<none>"}
  Command:       {application?.Exec ?? "<none>"}
  Args:          {(application.Args != null ? string.Join(" ", application.Args) : "<none>")}

  Config File:   {suluJsonPath}  

Check the configuration for this URL protocol.";
            OsServices.OpenText(msg);
            Serilog.Log.Error(msg, ex);
        }

        private string GetProtocol(string url)
        {
            var protocolSeparatorIndex = url.IndexOf("://");
            if (protocolSeparatorIndex == -1) return "";
            return url.Substring(0, protocolSeparatorIndex);
        }

        private ApplicationConfig GetApplicationForProtocol(SuluConfig config, string protocol)
        {
            var application = config.Protocols.FirstOrDefault(x => x.Protocol == protocol);
            if (application == null) return null;
            return config.Applications.FirstOrDefault(x => x.Id == application.AppId);
        }
    }
}
