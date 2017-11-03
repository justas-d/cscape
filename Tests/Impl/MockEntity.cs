using System;
using CScape.Models;
using CScape.Models.Game.Entity;

namespace CScape.Dev.Tests.Impl
{
    public sealed class MockEntity : IEntity
    {
        public bool Equals(IEntity other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IEntityHandle other)
        {
            throw new NotImplementedException();
        }

        public IEntityComponentContainer Components => throw new NotImplementedException();
        public IEntityHandle Handle => throw new NotImplementedException();
        public ILogger Log => throw new NotImplementedException();
        public string Name => throw new NotImplementedException();
        public IGameServer Server => throw new NotImplementedException();
        public bool AreComponentRequirementsSatisfied(out string message) => throw new NotImplementedException();

        public void SendMessage(IGameMessage message)
        {
            throw new NotImplementedException();
        }

        public void SystemMessage(string msg, ulong flags = (ulong)SystemMessageFlags.Normal)
        {
            throw new NotImplementedException();
        }
    }
}