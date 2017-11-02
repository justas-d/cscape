using System;
using CScape.Core.Game.Entities;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Segment
{
    public sealed class InitNpcSegment : IUpdateSegment
    {
        private readonly bool _needsUpdate;
        private readonly int _npcInstanceId;
        private readonly int _yDelta;
        private readonly int _xDelta;
        private readonly int _definitionId;

        public InitNpcSegment(
            [NotNull] INpcComponent npc,
            [NotNull] IEntity observerEntity, 
            bool needsUpdate)
        {
            if (npc == null) throw new ArgumentNullException(nameof(npc));
            if (observerEntity == null) throw new ArgumentNullException(nameof(observerEntity));

            var observerTransform = observerEntity.GetTransform();
            var npcTransform = npc.Parent.GetTransform();

            _yDelta = npcTransform.Y - observerTransform.Y;
            _xDelta = npcTransform.X - observerTransform.X;

            _npcInstanceId = npc.InstanceId;
            _needsUpdate = needsUpdate;
            _definitionId = npc.DefinitionId;
        }

        public void Write(OutBlob stream)
        {
            stream.WriteBits(14, _npcInstanceId);

            stream.WriteBits(5, _yDelta);
            stream.WriteBits(5, _xDelta); 

            stream.WriteBits(1, 1); // setpos?? flag // todo :  setpos flag
            stream.WriteBits(12, _definitionId);
            stream.WriteBits(1, _needsUpdate ? 1 : 0); 

        }
    }
}