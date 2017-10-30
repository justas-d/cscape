using System;
using System.Reflection;
using CScape.Models.Game.Skill;
using Newtonsoft.Json;

namespace CScape.Core.Database
{
    public sealed class SkillModelConverter : JsonConverter
    {
        public const string ExperienceProp = "Experience";
        public const string BoostProp = "Boost";

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var skill = (ISkillModel) value;
            writer.WriteStartObject();
            writer.WritePropertyName(ExperienceProp);
            writer.WriteValue(skill.Experience);
            writer.WritePropertyName(BoostProp);
            writer.WriteValue(skill.Boost);
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var skill = new SerializedSkillModel();
            var oldMissing = serializer.MissingMemberHandling;
            serializer.MissingMemberHandling = MissingMemberHandling.Ignore;
            serializer.Populate(reader, skill);
            serializer.MissingMemberHandling = oldMissing;
            return skill;
        }

        public override bool CanConvert(Type objectType) => typeof(ISkillModel).IsAssignableFrom(objectType);

        public override bool CanRead => true;
        public override bool CanWrite => true;
    }
}