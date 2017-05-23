using System;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.World
{
    public class GroundItem : WorldEntity
    {
        public (int id, int amount) Item { get; private set; }

        public GroundItem(
            [NotNull] IServiceProvider services, 
            (int id, int amount) item,
            IPosition pos) : base(services)
        {
            Item = item;
            Transform = ClientTransform.Create(this, pos);
        }

        public override void Update(IMainLoop loop)
        {
            // todo : remove after a certain period of time has passed.
        }

        public void UpdateAmount(int newAmount)
        {
            
        }
    }
}