using System;
using CScape.Core.Extensions;
using CScape.Core.Game.Entities.FacingData;
using CScape.Core.Game.Entity.InteractingEntity;
using CScape.Core.Game.Entity.Message;
using CScape.Models;
using CScape.Models.Extensions;
using CScape.Models.Game;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Entity.FacingData;
using CScape.Models.Game.Entity.InteractingEntity;
using CScape.Models.Game.Message;
using CScape.Models.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    /// <summary>
    /// Defines a way of tracking and transforming the location of server-side world entities.
    /// </summary>
    public sealed class Transform : EntityComponent, ITransform
    {
        public IInteractingEntity InteractingEntity { get; private set; }
            = NullInteractingEntity.Instance;

        public IFacingState FacingState { get; private set; }

        public DirectionDelta LastMovedDirection { get; private set; } = DirectionDelta.Noop;

        public const int MaxZ = 4;

        public int X { get; private set; } = 0;
        public int Y { get; private set; } = 0;
        public int Z { get; private set; } = 0;


        public IRegion Region { get; private set; }

        public IPlaneOfExistence PoE { get; private set; }

        public override int Priority => (int)ComponentPriority.Transform;

        public Transform([NotNull] IEntity parent)
            :base(parent)
        {
            FacingState = new NullFacingState(this);
            SwitchPoE(parent.Server.Overworld);
        }

        /// <summary>
        /// Cleanly switches the PoE of the entity.
        /// </summary>
        public void SwitchPoE(IPlaneOfExistence newPoe)
        {
            if (newPoe == null) throw new ArgumentNullException(nameof(newPoe));

            if (newPoe == PoE)
                return;

            var oldPoe = PoE;
            PoE?.RemoveEntity(this);
            PoE = newPoe;
            PoE.RegisterNewEntity(this);

            Parent.SendMessage(new PoeSwitchMessage(oldPoe, newPoe));
            UpdateRegion();
        }

        /// <summary>
        /// Forcibly teleports the transform to the given coordinates.
        /// </summary>
        public void Teleport(int x, int y, int z)
        {
            if (z > MaxZ) throw new ArgumentOutOfRangeException($"{nameof(z)} cannot be larger than {MaxZ}.");

            var oldPos = new ImmIntVec3(X, Y, Z);

            X = x;
            Y = y;
            Z = z;

            Parent.SendMessage(new TeleportMessage(oldPos, new ImmIntVec3(X, Y, Z)));
        }

        public void SetFacingDirection([NotNull]IFacingState state)
        {
            FacingState = state ?? throw new ArgumentNullException(nameof(state));

            Parent.SendMessage(new FacingDirectionMessage(state));
        }

        public void SetInteractingEntity([NotNull] IInteractingEntity ent)
        {
            InteractingEntity = ent ?? throw new ArgumentNullException(nameof(ent));

            Parent.SendMessage(new InteractingEntityMessage(ent));
        }

        private void UpdateRegion()
        {
            var region = PoE.GetRegion(X, Y) ?? throw new ArgumentNullException("PoE.GetRegion(X, Y)");

            if (Region == region) return;

            Region?.RemoveEntity(Parent.GetTransform());
            Region = region;
            Region.AddEntity(this);
        }

        public void SyncLocalsToGlobals(IClientPositionComponent client)
        {
            X = client.Base.X + client.Local.X;
            Y = client.Base.Y + client.Local.Y;

            UpdateRegion();

            Parent.Server.Services.ThrowOrGet<ILogger>()
                .Debug(this, "Synced client locals to globals.");
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            // TODO : handle ForcedMovement in ServerTransform,
            // TODO : handle ForcedMovement movement over time in a separate component
            switch (msg.EventId)
            {
                case (int) MessageId.Move:
                {
                    var data = msg.AsMove();
                    var delta = data.SumMovements();

                    X += delta.x;
                    Y += delta.y;

                    UpdateRegion();

                    /* 
                     * client implicitly sets the facing direction for the movign entity itself
                     * so there is no need to call SetFacingDirection and send out an entity-wide message
                     * we can just silently change the direction and if we need to ever check it, we
                     * will find good data no matter what.
                     */

                    // also we set the last moving direction here
                    if (data.IsWalking)
                    {
                        FacingState = new DirecionFacingState(data.Dir1, this);
                        LastMovedDirection = data.Dir1;
                    }
                    else
                    {
                        LastMovedDirection = data.Dir2;
                        FacingState = new DirecionFacingState(data.Dir2, this);
                    }
                        break;
                }
                case (int) MessageId.NewPlayerFollowTarget:
                {
                    var ent = msg.AsNewPlayerFollowTarget().Entity;
                    if (ent.IsDead())
                        break;
                    var player = ent.Get().GetPlayer();
                    if (player == null)
                        break;

                    SetInteractingEntity(new PlayerInteractingEntity(player));
                    break;
                }
                case (int) MessageId.SyncLocalsToGlobals:
                {
                    SyncLocalsToGlobals(Parent.AssertGetClientPosition());
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