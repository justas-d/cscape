using System;
using System.Collections.Generic;
using System.Reflection;
using CScape.Core.Game;
using CScape.Models.Game.Skill;
using Newtonsoft.Json;

namespace CScape.Core.Database
{
    public sealed class SkillIdConverter : JsonConverter
    {
        private readonly Dictionary<string, SkillID> _converter = new Dictionary<string, SkillID>();

        public SkillIdConverter(IServiceProvider services)
        {
            // populate converter dict
            var skills = services.ThrowOrGet<SkillDb>();
            foreach (var prop in skills.GetType().GetRuntimeProperties())
            {
                if (prop.PropertyType != typeof(SkillID)) continue;

                var skill = (SkillID)prop.GetValue(skills);
                _converter.Add(skill.Name, skill);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var skill = (SkillID)value;
            writer.WriteValue(skill.Name);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var str = reader.ReadAsString();
            return _converter[str];
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(SkillID);

        public override bool CanRead => true;
        public override bool CanWrite => true;
    }
}