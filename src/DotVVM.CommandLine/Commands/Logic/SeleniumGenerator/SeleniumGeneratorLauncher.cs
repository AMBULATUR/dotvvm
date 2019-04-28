﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DotVVM.CommandLine.Commands.Core;
using DotVVM.CommandLine.Commands.Logic.Compiler;
using DotVVM.CommandLine.Core;
using DotVVM.CommandLine.Core.Arguments;
using DotVVM.CommandLine.Core.Metadata;
using DotVVM.CommandLine.Core.Templates;
using DotVVM.Compiler;
using DotVVM.Utils.ProjectService;
using DotVVM.Utils.ProjectService.Operations.Providers;
using Newtonsoft.Json;

namespace DotVVM.CommandLine.Commands.Logic.SeleniumGenerator
{
    public class SeleniumGeneratorLauncher
    {
        private const string PageObjectsText = "PageObjects";

        public static void Start(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata,
            IResolvedProjectMetadata projectMetadata)
        {
            var metadata = JsonConvert.SerializeObject(JsonConvert.SerializeObject(dotvvmProjectMetadata));
            var exited = false;

            var processArgs = $"{CompilerConstants.Arguments.JsonOptions} {metadata} ";
            var i = 0;
            while (args[i] != null)
            {
                processArgs += $"{args[i]} ";
                i++;
            }

            var generator = DotvvmSeleniumGeneratorProvider.GetToolMetadata(projectMetadata);
            if (generator == null)
            {
                throw new Exception("Could not find DotVVM Selenium Generator tool. This tool is available in DotVVM 2.3.0 and newer. Make sure you have installed supported version.");
            }

            var executable = generator.MainModulePath;
            if (generator.Version == DotvvmToolExecutableVersion.DotNetCore)
            {
                executable = $"dotnet";
                processArgs = $"{JsonConvert.SerializeObject(generator.MainModulePath)} {processArgs}";
            }

            var processInfo = new ProcessStartInfo(executable) {
                RedirectStandardError = true,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                CreateNoWindow = false,
                Arguments = processArgs
            };

            var process = new Process {
                StartInfo = processInfo
            };

            process.OutputDataReceived += (sender, eventArgs) => {
                if (eventArgs?.Data?.StartsWith("#$") ?? false)
                {
                    exited = true;
                }

                Console.WriteLine(eventArgs?.Data);
            };

            process.ErrorDataReceived += (sender, eventArgs) => {
                if (eventArgs?.Data?.StartsWith("#$") ?? false)
                {
                    exited = true;
                }

                Console.WriteLine(eventArgs?.Data);
            };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            while (!exited)
            {
            }

            if (process.ExitCode != 0)
            {
                throw new InvalidCommandUsageException("Selenium generation failed.");
            }
        }
    }
}
