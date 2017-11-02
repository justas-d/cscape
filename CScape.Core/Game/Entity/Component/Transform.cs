using System;
using CScape.Core.Extensions;
using CScape.Core.Game.Entities.FacingData;
using CScape.Core.Game.Entity.Directions;
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
        // TODO : BUG: A follows B. B logs out then logs in. Result: A's interacting entity is still player B. Expected result: A's interacting entity should be the null default.
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

            UpdateRegion();
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
            var region = PoE.GetRegionByWorldCoordinate(X, Y) ?? throw new ArgumentNullException(nameof(PoE.GetRegionByWorldCoordinate));

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

        private void OnMove(MoveMessage data)
        {
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
        }

        private void OnNewFollowPlayer(EntityMessage data)
        {
            var ent = data.AsNewPlayerFollowTarget().Entity;
            if (ent.IsDead())
                return;

            var player = ent.Get().GetPlayer();
            if (player == null)
                return;

            SetInteractingEntity(new PlayerInteractingEntity(player));
        }

        private void OnBeginMovePath(BeginMovePathMessage msgData)
        {
            // TODO : HACK : only reset interacting entity if the new directions provider
            // is not a follow directions provider.
            // this is due to us receiving NewPlayerFollowTarget and then BeginMovePath,
            // the first one sets the interacting ent while the last resets it to null
            if (!(msgData.Directions is FollowDirectionProvider))
                SetInteractingEntity(NullInteractingEntity.Instance);
        }

        private bool DoesInteractingEntityNeedToBeReset()
        {
            if (InteractingEntity.Entity == null)
                return false;

            if (InteractingEntity.Entity.IsDead())
                return true;

            var vision = Parent.GetVision();
            if(vision == null)
                return false;

            return !vision.CanSee(InteractingEntity.Entity.Get());
        }

        private void OnUpdate()
        {
            if(DoesInteractingEntityNeedToBeReset())
                SetInteractingEntity(NullInteractingEntity.Instance);
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            // TODO : handle ForcedMovement in Transform,
            // TODO : handle ForcedMovement movement over time in a separate component
            switch (msg.EventId)
            {
                case (int) MessageId.FrameBegin:
                {
                    OnUpdate();
                    break;
                }
                case (int) MessageId.BeginMovePath:
                {
                    OnBeginMovePath(msg.AsBeginMovePath());
                    break;
                }
                case (int) MessageId.Move:
                {
                    OnMove(msg.AsMove());
                    break;
                }
                case (int) MessageId.NewPlayerFollowTarget:
                {
                    OnNewFollowPlayer(msg.AsNewPlayerFollowTarget());
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