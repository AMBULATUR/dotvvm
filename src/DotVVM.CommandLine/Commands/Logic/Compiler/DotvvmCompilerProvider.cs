﻿using System;
using System.Linq;
using DotVVM.Utils.ProjectService;
using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Operations.Providers;

namespace DotVVM.CommandLine.Commands.Logic.Compiler
{
    public class DotvvmCompilerProvider : DotvvmToolProvider
    {
        public static DotvvmToolMetadata GetCompilerMetadata(IResolvedProjectMetadata metadata)
        {
            var dotvvm = metadata.DotvvmProjectDependencies.First(s => s.Name.Equals("DotVVM", StringComparison.OrdinalIgnoreCase));
            if (dotvvm.IsProjectReference)
            {
                if ((metadata.TargetFramework & TargetFramework.NetFramework) > 0)
                {
                    return CreateMetadataOrDefault(
                        CombineDotvvmRepositoryRoot(metadata, dotvvm,
                            @"DotVVM.Compiler\bin\Debug\net461\DotVVM.Compiler.exe"),
                        DotvvmToolExecutableVersion.FullFramework);
                }

                return CreateMetadataOrDefault(
                    CombineDotvvmRepositoryRoot(metadata, dotvvm,
                        @"DotVVM.Compiler\bin\Debug\netcoreapp2.0\DotVVM.Compiler.dll"),
                    DotvvmToolExecutableVersion.DotNetCore);
            }
            if ((metadata.TargetFramework & TargetFramework.NetFramework) > 0)
            {
                return CreateMetadataOrDefault(CombineNugetPath(metadata, "tools\\DotVVM.Compiler.exe"),DotvvmToolExecutableVersion .FullFramework);

            }

            return CreateMetadataOrDefault(CombineNugetPath(metadata, "tools\\dnc\\DotVVM.Compiler.dll"), DotvvmToolExecutableVersion.DotNetCore);
        }
    }
}
