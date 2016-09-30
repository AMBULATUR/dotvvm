﻿namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// An interface for configuring DotVVM services.
    /// </summary>
    public interface IDotvvmBuilder
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection" /> where DotVVM services are configured.
        /// </summary>
        IServiceCollection Services { get; }
    }
}