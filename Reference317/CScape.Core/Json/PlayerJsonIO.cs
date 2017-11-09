using System;
using System.Diagnostics;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Component;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using Newtonsoft.Json;

namespace CScape.Core.Json
{
    public sealed class PlayerJsonIO
    {
        private readonly JsonConverter[] _converters;

        public PlayerJsonIO(IServiceProvider services)
        {
            _converters = new JsonConverter[]
            {
                new ItemContainerConverter(services),
                new SkillModelConverter(),
                new SkillIdConverter(services),
            };
        }

        public SerializablePlayerModel Deserialize(string data)
            => JsonConvert.DeserializeObject<SerializablePlayerModel>(data, _converters);

        public string Serialize(IEntity entity)
        {
            Debug.Assert(!entity.Handle.IsDead());

            var player = entity.AssertGetPlayer() as PlayerComponent;
            Debug.Assert(player != null);
            var compTransform = entity.GetTransform();
            var compSkills = entity.AssertGetSkills();
            var compInv = entity.AssertGetPlayerContainers();
            var health = entity.AssertGetHealth();

            var model = new SerializablePlayerModel(
                compTransform.X,
                compTransform.Y,
                compTransform.Z,
                compSkills.All,
                compInv.Backpack.Provider,
                compInv.Bank.Provider,
                compInv.Equipment.Provider,
                player.Username,
                player.TitleId,
                player.Apperance,
                health.Health);

            return JsonConvert.SerializeObject(model, Formatting.Indented, _converters);
        }
    }
}
