﻿namespace Sulu.Register
{
    class Application : IApplication
    {
        private Options Options { get; }
        private IRegistration Registration { get; }

        public Application(Options options, IRegistration registration)
        {
            Options = options;
            Registration = registration;
        }

        public int Run()
        {
            return Registration.Register(Options.Protocols, Options.Force) ? 0 : 1;
        }
    }
}
