using System;
using CScape.Core.Game.Entity;
using CScape.Core.Game.World;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    public sealed class MovementActionComponent : IEntityComponent
    {
        public Entity Parent { get; }

        public MovementActionComponent(Entity parent)
        {
            Parent = parent;
        }
        
        public void Update(IMainLoop loop)
        {
            throw new NotImplementedException();
        }

        public void ReceiveMessage(EntityMessage msg)
        {
            if (msg.Event == EntityMessage.EventType.ArrivedAtDestination)
            {
                
            }
        }
    }

    public sealed class TileMovementComponent : IEntityComponent
    {
        public enum MoveType
        {
            Noop,
            Walk,
            Run
        }

        [CanBeNull] public IDirectionsProvider Directions { get; private set; }

        /// <summary>
        /// The move action to be executed when direction provider is done.
        /// Will be reset to null after executing.
        /// </summary>
        [CanBeNull] public IMovementDoneAction MoveAction { get; set; }

        public Entity Parent { get; }

        /// <summary>
        /// The direction deltas in which this entity last moved successfully.
        /// </summary>
        public (sbyte x, sbyte y) LastMovedDirection { get; private set; }

        public Direction LastDirection1 { get; private set; }
        public Direction LastDirection2 { get; private set; }

        public MoveType LastMoveType { get; private set; }

        public TileMovementComponent(Entity parent)
        {
            Parent = parent;
        }

        public void SetDirectionProvider([NotNull] IDirectionsProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            // if we're overwriting, we have to dispose existing provider.
            DisposeDirections();

            Directions = provider;
            
        }

        private void DisposeDirections()
        {
            MoveAction = null;
            if (Directions != null)
            {
                Directions.Dispose();
                _directions = null;
            }
        }

        public void Update(IMainLoop loop)
        {
            void Reset()
            {
                MoveUpdate.Type = MovementController.MoveUpdateData.MoveType.Noop;
                DisposeDirections();
            }

            // verify whether we have an active direction provider
            if (Directions != null && Directions.IsDone())
            {
                // assign to local to allow MoveAction to reset itself in Execute()
                var moveAction = MoveAction;
                Reset();
                moveAction?.Execute();
                return;
            }

            if (Directions == null)
            {
                Reset();
                return;
            }

            // helper methods
            var didMove = false;

            bool IsNoop((sbyte x, sbyte y) d)
                => d.x == DirectionHelper.NoopDelta.x && d.y == DirectionHelper.NoopDelta.y;

            void Move((sbyte, sbyte) d, out byte updateDir)
            {
                updateDir = (byte)DirectionHelper.GetDirection(d);
                Entity.Transform.Move(d.Item1, d.Item2);
                LastMovedDirection = d;
                didMove = true;
            }

            void Walk((sbyte, sbyte) d)
            {
                Move(d, out MoveUpdate.Dir1);
                MoveUpdate.Type = MovementController.MoveUpdateData.MoveType.Walk;
            }

            // get first movement since we've got an active direction
            var d1 = Directions.GetNextDir();

            if (IsRunning)
            {
                // validate d1
                if (!IsNoop(d1))
                {
                    // d1 is valid,
                    // immediatelly walk so that Directions.IsDone() gives valid data
                    Walk(d1);

                    // 2 moves per update if we're running
                    if (!Directions.IsDone())
                    {
                        // get the second one
                        var d2 = Directions.GetNextDir();

                        // validate
                        if (!IsNoop(d2))
                        {
                            // it's good, move
                            Move(d2, out MoveUpdate.Dir2);
                            MoveUpdate.Type = MovementController.MoveUpdateData.MoveType.Run;
                        }
                        // if second move is a noop, who cares. We already walked.
                    }
                }
            }
            // if we're not running, we're walking.
            else
            {
                if (!IsNoop(d1))
                    // active direction provided a valid direction, execute.
                    Walk(d1);
                else
                    // active direction provided no directions, be idle.
                    MoveUpdate.Type = MovementController.MoveUpdateData.MoveType.Noop;
            }

            if (didMove)
            {
                Entity.NeedsSightEvaluation = true;
                Entity.OnMoved();
            }

            _loop.Movement.Enqueue(Entity);

        }

        public void ReceiveMessage(EntityMessage msg)
        {
            
        }
    }


    public sealed class PlayerComponent : IEntityComponent, IEquatable<PlayerComponent>
    {
        public int PlayerId { get; }

        [NotNull]
        public Entity Parent { get; }

        [NotNull]
        public string Username { get; }

        public PlayerComponent([NotNull] Entity parent, [NotNull] string username
            , int playerId)
        {
            PlayerId = playerId;
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Username = username ?? throw new ArgumentNullException(nameof(username));
        }

        public void Update(IMainLoop loop)
        {

        }

        public void ReceiveMessage(EntityMessage msg)
        {

        }

        public bool Equals(PlayerComponent other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Username, other.Username, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is PlayerComponent && Equals((PlayerComponent) obj);
        }

        public override int GetHashCode()
        {
            return Username.GetHashCode() * 31;
        }
    }
}
