﻿using System.Collections.Generic;
using System.Diagnostics;

namespace Sulu.Platform
{
    public interface IOsServices
    {
        Process OpenDefault(string file);

        Process Execute(string binary, IEnumerable<string> args);

        /// <summary>
        /// Open a text editor to display message
        /// </summary>
        void OpenText(string message);

        bool IsAdministrator();
    }
}
