using System;
using JetBrains.Annotations;

namespace CScape.Models.Data
{
    public interface IConfigurationService : IDisposable
    {
        /// <summary>
        /// Adds a <see cref="value"/> mapped to the <see cref="key"/> into the config.
        /// </summary>
        /// <returns>True if added succesfully, false otherwise.</returns>
        bool Add([NotNull] string key, [NotNull] string value);

        /// <summary>
        /// Retrieves a value that was mapped to the given <see cref="key"/>
        /// </summary>
        /// <returns>The mapped value or null if there is none.</returns>
        [CanBeNull]
        string Get([NotNull] string key);

        /// <summary>
        /// Reloads the config lookup.
        /// </summary>
        void Reload();
    }
}