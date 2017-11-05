using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using CScape.Core.Extensions;
using CScape.Core.Utility;
using CScape.Models;
using CScape.Models.Data;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CScape.Core.Json
{
    public sealed class JsonConfigurationService : IConfigurationService
    {
        private ImmutableDictionary<string, string> _lookup = ImmutableDictionary<string, string>.Empty;

        private Lazy<ILogger> _log;
        private ILogger Log => _log.Value;

        private bool _isDisposed = false;

        [NotNull]
        public string FilePath { get; }

        public JsonConfigurationService(
            [NotNull] IServiceProvider services, 
            [NotNull] string filePath)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            _log = services.GetLazy<ILogger>();

            Reload();
        }

        public void Reload()
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

            _lookup = immDict;

            Log.Normal(this, "Overwrote config with the one from disk.");
        }

        private bool DoesConfigFileExist()
        {
            if (!File.Exists(FilePath))
            {
                Log.Warning(this, $"Could not find a config file located at {FilePath}");
                return false;
            }
            return true;
        }

        public bool Add(string key, string value)
        {
            return ImmutableInterlocked.TryAdd(ref _lookup, key, value);
        }

        public string Get(string key)
        {
            _lookup.TryGetValue(key, out var retval);
            return retval;
        }

        public void Dispose()
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
            var dict = _lookup.ToDictionary(kvp => kvp.Key);
            var json = JsonConvert.SerializeObject(dict);

            File.WriteAllText(FilePath, json);

            Log.Normal(this, $"Saved json config to {FilePath}");
        }
    }
}