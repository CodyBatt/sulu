﻿using Autofac;
using Sulu.Platform;
using Sulu.Util;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Sulu
{
    public static class Ioc
    {
        public static IContainer RegisterApplication(Serilog.ILogger logger)
        {
            var builder = new ContainerBuilder();

            // Standard type registrations
            builder.RegisterInstance(logger).As<Serilog.ILogger>().SingleInstance();
            builder.RegisterType<CommandLineHandler>().As<ICommandLineParser>().SingleInstance();
            builder.RegisterType<Registration>().As<IRegistration>().SingleInstance();
            builder.RegisterType<SuluConfiguration>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ProtocolHandlerFactory>().AsImplementedInterfaces().SingleInstance();

            // Perform platform-specific registrations here
            builder.RegisterPlatformSpecificComponents();

            // Register everything that derives from Autofac.Module
            builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());
            return builder.Build();
        }

        private static void RegisterPlatformSpecificComponents(this ContainerBuilder builder)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                builder.RegisterWindowsComponents();
            }
            else
            {
                // Register the "unsupported platform" components with preserve existing defaults, so they only
                // take effect if nothing else registered to implement the service
                builder.RegisterType<UnsupportedPlatformRegistrar>().AsImplementedInterfaces().SingleInstance().PreserveExistingDefaults();
            }

            builder.RegisterType<UserInteraction>().AsImplementedInterfaces().SingleInstance().PreserveExistingDefaults();
            builder.RegisterType<OsServicesBase>().AsImplementedInterfaces().SingleInstance().PreserveExistingDefaults();
        }

        private static void RegisterWindowsComponents(this ContainerBuilder builder)
        {
            //builder.RegisterType<GuiUserInteraction>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<WindowsBasicProtocolRegistrar>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<WindowsProtocolRegistrar>().AsImplementedInterfaces().SingleInstance();
        }
    }
}
