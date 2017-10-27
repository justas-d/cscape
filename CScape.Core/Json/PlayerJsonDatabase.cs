﻿using System;
using System.Collections.Generic;
using System.IO;
using CScape.Models;
using CScape.Models.Game.Entity.Component;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CScape.Core.Database
{
    public sealed class PlayerJsonDatabase
    {
        [NotNull]
        private readonly PlayerJsonIO _serializer;

        private const string SaveDir = "players/";
        private const string PasswordsDir = "passwd.json";

        private Dictionary<string, string> _pwdLookup;

        private readonly ILogger _log;

        public PlayerJsonDatabase(IServiceProvider services)
        {
            _serializer = new PlayerJsonIO(services);
            _log = services.ThrowOrGet<ILogger>();
            LoadPwdLookup();
        }

        private void LoadPwdLookup()
        {
            if (File.Exists(PasswordsDir))
            {
                _pwdLookup = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(PasswordsDir));
            }
            else
            {
                _pwdLookup = new Dictionary<string, string>();
            }
        }

        private string MakeFileDir(string username)
            => Path.Combine(SaveDir, username + ".json");

        public bool IsValidPassword(string username, string password)
        {
            if (!_pwdLookup.ContainsKey(username))
            {
                _log.Warning(this, $"Tried to check if pwd is valid for username {username} but the username was not found in pwd lookup.");
                return false;
            }

            return _pwdLookup[username].Equals(password, StringComparison.Ordinal);
        }

        public bool PlayerExists(string username) => File.Exists(MakeFileDir(username));

        public void Save([NotNull] IPlayerComponent player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            var data = _serializer.Serialize(player.Parent);

            File.WriteAllText(MakeFileDir(player.Username), data);
        }

        [CanBeNull]
        public SerializablePlayerModel Load([NotNull] string username)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));

            if (!PlayerExists(username)) return null;

            return _serializer.Deserialize(File.ReadAllText(MakeFileDir(username)));
        }
    }
}