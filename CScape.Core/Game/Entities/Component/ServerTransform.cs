using System;
using CScape.Core.Game.Entities.FacingData;
using CScape.Core.Game.Entities.InteractingEntity;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Game.World;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    /// <summary>
    /// Defines a way of tracking and transforming the location of server-side world entities.
    /// </summary>
    public sealed class ServerTransform : EntityComponent, IPosition
    {
        [NotNull]
        public IInteractingEntity InteractingEntity { get; private set; }
            = NullInteractingEntity.Instance;

        [NotNull]
        public IFacingData FacingData { get; private set; }

        public DirectionDelta LastMovedDirection { get; private set; }

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

        public override int Priority { get; }

        public ServerTransform([NotNull] Entity parent)
            :base(parent)
        {
            FacingData = new DefaultDirection(this);
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
                new GameMessage(
                    this, 
                    GameMessage.Type.PoeSwitch, 
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

            Parent.SendMessage(
                new GameMessage(
                    this,
                    GameMessage.Type.Teleport,
                    new TeleportMessageData(oldPos, newPos)));

        }

        // TODO : use SetFacingDirection
        public void SetFacingDirection([NotNull]IFacingData data)
        {
            FacingData = data ?? throw new ArgumentNullException(nameof(data));

            Parent.SendMessage(
                new GameMessage(
                    this, 
                    GameMessage.Type.NewFacingDirection, 
                    data));
        }

        public void SetInteractingEntity([NotNull] IInteractingEntity ent)
        {
            InteractingEntity = ent ?? throw new ArgumentNullException(nameof(ent));

            Parent.SendMessage(
                new GameMessage(
                    this, GameMessage.Type.NewInteractingEntity,
                    ent));
        }

        private void UpdateRegion()
        {
            var region = PoE.GetRegion(X, Y);

            if (Region == region) return;

            Region?.RemoveEntity(Parent.GetTransform());
            Region = region;
            Region.AddEntity(this);
        }

        public void SyncLocalsToGlobals(ClientPositionComponent client)
        {
            X = client.Base.x + client.Local.x;
            X = client.Base.y + client.Local.y;

            UpdateRegion();

            Parent.Server.Services.ThrowOrGet<ILogger>()
                .Debug(this, "Synced client locals to globals.");
        }

        public override void ReceiveMessage(GameMessage msg)
        {
            // TODO : handle ForcedMovement in ServerTransform,
            // TODO : handle ForcedMovement movement over time in a separate component
            switch (msg.Event)
            {
                case GameMessage.Type.Move:
                {
                    var data = msg.AsMove();
                    var delta = data.SumMovements();

                    X += delta.x;
                    Y += delta.y;

                    /* 
                     * client implicitly sets the facing direction for the movign entity itself
                     * so there is no need to call SetFacingDirection and send out an entity-wide message
                     * we can just silently change the direction and if we need to ever check it, we
                     * will find good data no matter what.
                     */

                    // also we set the last moving direction here
                    if (data.IsWalking)
                    {
                        FacingData = new FacingDirection(data.Dir1, this);
                        LastMovedDirection = data.Dir1;
                    }
                    else
                    {
                        LastMovedDirection = data.Dir2;
                        FacingData = new FacingDirection(data.Dir2, this);
                    }
                    break;
                }
                case GameMessage.Type.NewPlayerFollowTarget:
                {
                    var targ = msg.AsNewFollowTarget();
                    InteractingEntity = new PlayerInteractingEntity(targ);
                    break;
                }
            }
        }

        public bool Equals(IPosition other)
        {
            return 
                other.X == X && 
                other.Y == Y && 
                other.Z == Z;
        }
    }
}