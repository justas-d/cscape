using System;
using System.Collections.Generic;
using CScape.Game.Worldspace;

namespace CScape.Game.Entity
{
    public sealed class MovementController
    {
        public Transform Position { get; }
        public bool IsRunning { get; set; }

        private readonly Queue<Direction> _movementQueue = new Queue<Direction>();

        public MovementController(Transform position)
        {
            Position = position;
        }

        public void ResetMovementQueue() => _movementQueue.Clear();
        public void QueueMovement(Direction direction) => _movementQueue.Enqueue(direction);


        public static (sbyte x, sbyte y) GetDelta(Direction dir)
        {
            const sbyte dn = 1;
            const sbyte ds = -1;
            const sbyte dw = -1;
            const sbyte de = 1;
            switch (dir)
            {
                case Direction.None:
                    return (0, 0);
                case Direction.NorthWest:
                    return (dw, dn);
                case Direction.North:
                    return (0, dn);
                case Direction.NorthEast:
                    return (de, dn);
                case Direction.West:
                    return (dw, 0);
                case Direction.East:
                    return (de, 0);
                case Direction.SouthWest:
                    return (dw, ds);
                case Direction.South:
                    return (0, ds);
                case Direction.SouthEast:
                    return (de, ds);
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }
        }

        public void Update()
        {
            // no op
            if (_movementQueue.Count == 0)
            {
                MoveUpdate.Type = MoveUpdateData.MoveType.Noop;
                return;
            }

            void Move(Direction dir, out byte updateDir)
            {
                updateDir = (byte)dir;
                Position.TransformLocals(GetDelta(dir));
            }

            // walk
            if (IsRunning && _movementQueue.Count == 1 || !IsRunning)
            {
                Move(_movementQueue.Dequeue(), out MoveUpdate.Dir1);
            }
            // run
            if (IsRunning && _movementQueue.Count >= 2)
            {
                Move(_movementQueue.Dequeue(), out MoveUpdate.Dir1);
                Move(_movementQueue.Dequeue(), out MoveUpdate.Dir2);
            }
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