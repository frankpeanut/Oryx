﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    /// <summary>
    /// Configures the service provider, where all dependency injection is setup.
    /// </summary>
    internal class ServiceProviderBuilder
    {
        private IServiceCollection serviceCollection;

        public ServiceProviderBuilder(string logFilePath = null, IConsole console = null)
        {
            var disableTelemetryEnvVariableValue = Environment.GetEnvironmentVariable(
               LoggingConstants.OryxDisableTelemetryEnvironmentVariableName);
            _ = bool.TryParse(disableTelemetryEnvVariableValue, out bool disableTelemetry);
            var config = new TelemetryConfiguration();
            var aiKey = disableTelemetry ? string.Empty : Environment.GetEnvironmentVariable(
                LoggingConstants.ApplicationInsightsConnectionStringKeyEnvironmentVariableName);
            if (!string.IsNullOrEmpty(aiKey))
            {
                config.ConnectionString = aiKey;
            }

            this.serviceCollection = new ServiceCollection();
            this.serviceCollection
                .AddBuildScriptGeneratorServices()
                .AddCliServices(console)
                .AddLogging(builder =>
                {
                    if (!string.IsNullOrEmpty(aiKey))
                    {
                    builder.AddApplicationInsights(
                            configureTelemetryConfiguration: (c) => c.ConnectionString = aiKey,
                        configureApplicationInsightsLoggerOptions: (options) => { });
                    }

                    builder.SetMinimumLevel(Extensions.Logging.LogLevel.Trace);
                    var pathFormat = !string.IsNullOrWhiteSpace(logFilePath) ? logFilePath : LoggingConstants.DefaultLogPath;
                    builder.AddFile(pathFormat);
                })
                .AddSingleton<TelemetryClient>(new TelemetryClient(config));
        }

        public ServiceProviderBuilder ConfigureServices(Action<IServiceCollection> configure)
        {
            configure(this.serviceCollection);
            return this;
        }

        public ServiceProviderBuilder ConfigureScriptGenerationOptions(Action<BuildScriptGeneratorOptions> configure)
        {
            this.serviceCollection.Configure<BuildScriptGeneratorOptions>(opts => configure(opts));
            return this;
        }

        public IServiceProvider Build()
        {
            return this.serviceCollection.BuildServiceProvider();
        }
    }
}