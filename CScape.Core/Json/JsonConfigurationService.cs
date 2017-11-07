using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CScape.Core.Json
{
    public sealed class JsonConfigurationService : InMemoryConfigurationService
    {

        private bool _isDisposed = false;

        [NotNull]
        public string FilePath { get; }

        public JsonConfigurationService(
            [NotNull] IServiceProvider services,
            [NotNull] string filePath)
            : base(services)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            Reload();
        }

        public override void Reload()
        {
            if (!DoesConfigFileExist())
                return;

            try
            {
                LoadConfigThenOverwriteLookup();
            }
            catch (Exception e)
            {
                Log.Exception(this, "Failed loading config file.", e);
            }
        }

        private void LoadConfigThenOverwriteLookup()
        {
            var fileContents = File.ReadAllText(FilePath);
            var rawDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContents);

            var immDict = rawDict.ToImmutableDictionary();

            Lookup = immDict;

            Log.Normal(this, "Overwrote config with the one from disk.");
        }

        private bool DoesConfigFileExist()
        {
            if (!File.Exists(FilePath))
            {
                Log.Warning(this, $"Could not find a config file located at {Path.GetFullPath(FilePath)}");
                return false;
            }
            return true;
        }


        public override void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                WriteToDisk();
            }
        }

        private void Save()
        {
            try
            {
                WriteToDisk();
            }
            catch (Exception e)
            {
                Log.Exception(this, $"Failed saving json configuration to disk at {FilePath}", e);
            }
        }

        private void WriteToDisk()
        {
            var dict = Lookup.ToDictionary(kvp => kvp.Key);
            var json = JsonConvert.SerializeObject(dict);

            File.WriteAllText(FilePath, json);

            Log.Normal(this, $"Saved json config to {FilePath}");
        }
    }
}