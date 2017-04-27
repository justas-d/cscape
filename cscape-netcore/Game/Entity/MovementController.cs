using System;
using System.Collections;
using System.Collections.Generic;
using CScape.Game.World;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    public sealed class ByReferenceWithDeltaWaypointsDirectionsProvider : IDirectionsProvider
    {
        private class DeltaEnumerator : IEnumerator<(sbyte, sbyte)>
        {
            public bool MoveNext()
            {
                throw new NotImplementedException();
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public(sbyte, sbyte) Current { get; }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        private (int x, int y) _local;
        private readonly (int x, int y) _reference;
        private readonly (sbyte x, sbyte y)[] _deltaWaypoints;
        private  (int, int) _target;
        private readonly IEnumerator<(sbyte, sbyte)> _nextProvider;
        private int _idx;
        private bool _isFirst = true;

        [NotNull]
        public Transform Pos { get; }

        public ByReferenceWithDeltaWaypointsDirectionsProvider(
            [NotNull] Transform pos,
            (int, int) reference,
            (sbyte, sbyte)[] deltaWaypoints)
        {
            _reference = reference;
            _target = reference;
            _deltaWaypoints = deltaWaypoints;
            Pos = pos;
            _local = (Pos.LocalX, Pos.LocalY);

            // create ienumerable
            _nextProvider = NextProvider();
        }

        public (sbyte, sbyte) GetNextDir()
        {
            _nextProvider.MoveNext();
            return _nextProvider.Current;
        }

        public bool IsDone()
        {
            if (_isFirst)
                return false;

            return _deltaWaypoints.Length <= _idx;
        }

        public void Dispose()
        {
            _nextProvider.Dispose();
        }

        private IEnumerator<(sbyte x, sbyte y)> NextProvider()
        {
            if (IsDone())
                yield break;

            if(!_isFirst)
                _target = (_reference.x + _deltaWaypoints[_idx].x, _reference.y + _deltaWaypoints[_idx].y);

            foreach (var d in Interpolate(_local, _target))
            {
                yield return d;

                // keep track of locals ourselves incase transform 
                // update dx and dy kick in and changes locals in the transform
                _local.x += d.x;
                _local.y += d.y;
            }

            if(!_isFirst)
                ++_idx;

            _isFirst = false;
        }

        private IEnumerable<(sbyte x, sbyte y)> Interpolate((int x, int y) local, (int x, int y) target)
        {
            var max = Math.Max(Math.Abs(local.x - target.x), Math.Abs(local.y - target.y));
            if (max == 0)
                yield break;

            var diffX = local.x < target.x ? (sbyte)1 : (sbyte)-1;
            var diffY = local.y < target.y ? (sbyte)1 : (sbyte)-1;

            for (var i = 0; i < max; i++)
            {
                sbyte dX = 0;
                sbyte dY = 0;

                if (local.x != target.x)
                    local.x += dX = diffX;

                if (local.y != target.y)
                    local.y += dY = diffY;

                yield return (dX, dY);
            }
        }
    }

    public interface IDirectionsProvider
    {
        (sbyte, sbyte) GetNextDir();
        bool IsDone();
        void Dispose();
    }

    public sealed class MovementController
    {
        [CanBeNull] private IDirectionsProvider _directions;
        public AbstractEntity Entity { get; }
        public bool IsRunning { get; set; }

        public MovementController(AbstractEntity entity)
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
                Move(Directions.GetNextDir(), out MoveUpdate.Dir2);
                MoveUpdate.Type = MoveUpdateData.MoveType.Run;

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