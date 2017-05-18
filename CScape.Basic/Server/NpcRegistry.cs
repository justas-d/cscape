using System;
using CScape.Core.Game.Entity;

namespace CScape.Basic.Server
{
    public class NpcRegistry : AbstractEntityRegistry<int, Npc>
    {
        public NpcRegistry(IServiceProvider services) : base(services) { }
        protected override int GetId(Npc val) => val.UniqueNpcId;
    }
}