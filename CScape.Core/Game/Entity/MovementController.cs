using System;
using CScape.Core.Game.World;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public sealed class MovementController
    {
        [CanBeNull] private IDirectionsProvider _directions;
        private readonly IMainLoop _loop;

        public IMovingEntity Entity { get; }
        public bool IsRunning { get; set; }

        /// <summary>
        /// The direction in which this entity last moved succesfully.
        /// </summary>
        public (sbyte x, sbyte y) LastMovedDirection { get; private set; }

        public MovementController(IServiceProvider services, IMovingEntity entity)
        {
            Entity = entity;
            _loop = services.ThrowOrGet<IMainLoop>();
        }

        [CanBeNull] public IDirectionsProvider Directions
        {
            get => _directions;
            set
            {
                // if we're overwriting, we have to dispose existing provider.
                DisposeDirections();

                _directions = value;
                _loop.Movement.Enqueue(Entity);
            }
        }

        /// <summary>
        /// The move action to be executed when direction provider is done.
        /// Will be reset to null after executing.
        /// </summary>
        [CanBeNull] public IMovementDoneAction MoveAction { get; set; }

        public void DisposeDirections()
        {
            MoveAction = null;
            if (Directions != null)
            {
                Directions.Dispose();
                _directions = null;
            }
        }

        public void Update()
        {
            void Reset()
            {
                MoveUpdate.Type = MoveUpdateData.MoveType.Noop;
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
                updateDir = (byte) DirectionHelper.GetDirection(d);
                Entity.Transform.Move(d.Item1, d.Item2);
                LastMovedDirection = d;
                didMove = true;
            }

            void Walk((sbyte, sbyte) d)
            {
                Move(d, out MoveUpdate.Dir1);
                MoveUpdate.Type = MoveUpdateData.MoveType.Walk;
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
                    if(!Directions.IsDone())
                    {
                        // get the second one
                        var d2 = Directions.GetNextDir();

                        // validate
                        if (!IsNoop(d2))
                        {
                            // it's good, move
                            Move(d2, out MoveUpdate.Dir2);
                            MoveUpdate.Type = MoveUpdateData.MoveType.Run;
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
                    MoveUpdate.Type = MoveUpdateData.MoveType.Noop;
            }

            if (didMove)
            {
                Entity.NeedsSightEvaluation = true;
                Entity.OnMoved();
            }
                
            _loop.Movement.Enqueue(Entity);
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

            public void Reset()
            {
                Type = MoveType.Noop;
                Dir1 = Dir2 = 0;
            }
        }
    }
}