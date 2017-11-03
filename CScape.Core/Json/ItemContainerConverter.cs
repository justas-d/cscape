using System;
using System.Collections.Generic;
using System.Reflection;
using CScape.Core.Extensions;
using CScape.Models.Game.Item;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CScape.Core.Json
{
    public sealed class ItemContainerConverter : JsonConverter
    {
        private readonly ItemDatabase _db;

        public const string IdProp = "Id";
        public const string AmountProp = "Amount";


        public ItemContainerConverter(IServiceProvider services)
        {
            _db = services.ThrowOrGet<ItemDatabase>();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var item = (ItemStack) value;

            writer.WriteStartObject();

            writer.WritePropertyName(IdProp);
            writer.WriteValue(item.Id.ItemId);

            writer.WritePropertyName(AmountProp);
            writer.WriteValue(item.Amount);

            writer.WriteEndObject();

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var items = new List<ItemStack>();
            foreach (var token in JArray.Load(reader))
            {
                var id = token[IdProp].ToObject<int>();
                var amnt = token[AmountProp].ToObject<int>();

                items.Add(new ItemStack(_db.Get(id), amnt));
            }

            return items.ToArray();
        }

        public override bool CanConvert(Type objectType) => typeof(ItemStack).IsAssignableFrom(objectType);

        public override bool CanRead => true;
        public override bool CanWrite => true;
    }
}