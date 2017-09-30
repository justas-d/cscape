using System;
using CScape.Core.Game.NewEntity;
using CScape.Core.Game.World;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    /// <summary>
    /// Defines a way of tracking and transforming the location of server-side world entities.
    /// </summary>
    public sealed class ServerTransform : IPosition, IEntityComponent
    {
        public const int MaxZ = 4;

        public int X { get; private set; } = 0;
        public int Y { get; private set; } = 0;
        public int Z { get; private set; } = 0;

        /// <summary>
        /// Returns the current PoE region this transform is stored in.
        /// </summary>
        [NotNull] public Region Region { get; private set; }

        /// <summary>
        /// The entities current PoE
        /// </summary>
        [NotNull] public PlaneOfExistence PoE { get; private set; }

        public NewEntity.Entity Parent { get; }

        public bool NeedsSightEvaluation { get; set; } = true;

        public ServerTransform([NotNull] NewEntity.Entity parent)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            SwitchPoE(parent.Server.Overworld);
        }

        /// <summary>
        /// Cleanly switches the PoE of the entity.
        /// </summary>
        public void SwitchPoE([NotNull] PlaneOfExistence newPoe)
        {
            if (newPoe == null) throw new ArgumentNullException(nameof(newPoe));

            if (newPoe == PoE)
                return;

            var oldPoe = PoE;
            PoE?.RemoveEntity(this);
            PoE = newPoe;
            PoE.RegisterNewEntity(this);

            Parent.SendMessage(
                new EntityMessage(
                    this, 
                    EntityMessage.EventType.PoeSwitch, 
                    new PoeSwitchMessageData(oldPoe, newPoe)));

            UpdateRegion();
        }

        /// <summary>
        /// Forcibly teleports the transform to the given coordinates.
        /// </summary>
        public void Teleport(int x, int y, int z)
        {
            if (z > MaxZ) throw new ArgumentOutOfRangeException($"{nameof(z)} cannot be larger than {MaxZ}.");

            var oldPos = (X, Y, Z);
            var newPos = (x, y, z);

            X = x;
            Y = y;
            Z = z;

            NeedsSightEvaluation = true;

            Parent.SendMessage(
                new EntityMessage(
                    this,
                    EntityMessage.EventType.Teleport,
                    new TeleportMessageData(oldPos, newPos)));
        }


        private void UpdateRegion()
        {
            var region = PoE.GetRegion(X, Y);

            if (Region == region) return;

            Region?.RemoveEntity(Parent.GetTransform());
            Region = region;
            Region.AddEntity(this);

            NeedsSightEvaluation = true;
        }

        public void SyncLocalsToGlobals(ClientPositionComponent client)
        {
            X = client.Base.x + client.Local.x;
            X = client.Base.y + client.Local.y;

            NeedsSightEvaluation = true;
            UpdateRegion();

            Parent.Server.Services.ThrowOrGet<ILogger>()
                .Debug(this, "Synced client locals to globals.");
        }

        public void Update(IMainLoop loop)
        {
            throw new NotImplementedException();
        }

        public void ReceiveMessage(EntityMessage msg)
        {
            if (msg.Event == EntityMessage.EventType.Move)
            {
                var delta = msg.AsMove().SumMovements();

                X += delta.x;
                Y += delta.y;

                NeedsSightEvaluation = true;

            }
        }
    }
}