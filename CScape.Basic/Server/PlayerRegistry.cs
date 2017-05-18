using System;
using CScape.Core.Game.Entity;

namespace CScape.Basic.Server
{
    public class PlayerRegistry : AbstractEntityRegistry<short, Player>
    {
        public PlayerRegistry(IServiceProvider services) : base(services) { }
        protected override short GetId(Player val) => val.Pid;
    }
}