using CScape.Game.World;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    public sealed class MovementController
    {
        [CanBeNull] private IDirectionsProvider _directions;
        public IMovingEntity Entity { get; }
        public bool IsRunning { get; set; }

        public MovementController(IMovingEntity entity)
        {
            Entity = entity;
        }

        [CanBeNull] public IDirectionsProvider Directions
        {
            get => _directions;
            set
            {
                _directions = value;
                Entity.Server.Loop.Movement.Enqueue(Entity);
            }
        }

        public void Update()
        {
            // no op
            if (Directions == null || Directions.IsDone())
            {
                MoveUpdate.Type = MoveUpdateData.MoveType.Noop;
                _directions = null;
                return;
            }

            void Move((sbyte, sbyte) d, out byte updateDir)
            {
                updateDir = (byte) DirectionHelper.GetDirection(d);
                Entity.Position.TransformLocals(d);
            }

            if (IsRunning)
            {
                Move(Directions.GetNextDir(), out MoveUpdate.Dir1);

                // if no more dirs to target, but we're running, finish the run by just walking
                if (Directions.IsDone())
                    MoveUpdate.Type = MoveUpdateData.MoveType.Walk;
                else // continue running if we still have dirs
                {
                    Move(Directions.GetNextDir(), out MoveUpdate.Dir2);
                    MoveUpdate.Type = MoveUpdateData.MoveType.Run;
                }

            }
            else
            {
                Move(Directions.GetNextDir(), out MoveUpdate.Dir1);
                MoveUpdate.Type = MoveUpdateData.MoveType.Walk;
            }

            Entity.Server.Loop.Movement.Enqueue(Entity);
        }

        internal MoveUpdateData MoveUpdate { get; } = new MoveUpdateData();

        internal class MoveUpdateData
        {
            public enum MoveType
            {
                Noop,
                Walk,
                Run
            }

            public MoveType Type;

            public byte Dir1;
            public byte Dir2;
        }
    }
}