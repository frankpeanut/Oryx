﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class RunScriptCommandProperty : CommandBaseProperty
    {
        public string AppDir { get; set; } = ".";

        public string PlatformName { get; set; }

        public string PlatformVersion { get; set; }

        public string OutputPath { get; set; }

        public string[] RemainingArgs { get; }
    }
}
