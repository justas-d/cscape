using System;
using CScape.Core.Game.World;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    // todo : health interface
    /// <summary>
    /// Defines an AI pawn NPC.
    /// </summary>
    public class Npc : WorldEntity, IMovingEntity, IDamageable
    {
        #region Sync
        [Flags]
        public enum UpdateFlags : byte
        {
            Animation = 0x10,
            PrimaryHit = 8,
            ParticleEffect = 0x80,
            InteractingEntity = 0x20,
            Text = 1,
            SecondaryHit = 0x40,
            Definition = 2,
            FacingCoordinate = 4
        }

        public UpdateFlags TickFlags { get; private set; }

        public (sbyte x, sbyte y) LastMovedDirection { get; set; }

        public HitData SecondaryHit { get; private set; }
        public HitData PrimaryHit { get; private set; }

        // todo : set npc max health based on their npc definition data. (by lookup)
        public byte MaxHealth { get; }
        public byte CurrentHealth { get; private set; }

        public bool Damage(byte dAmount, HitType type, bool secondary)
        {
            var hit = HitData.Calculate(this, type, dAmount);
            CurrentHealth = hit.CurrentHealth;

            if (secondary)
            {
                SecondaryHit = hit;
                TickFlags |= UpdateFlags.SecondaryHit;
            }
            else
            {
                PrimaryHit = hit;
                TickFlags |= UpdateFlags.PrimaryHit;
            }

            return CurrentHealth == 0;
        }

        private ParticleEffect _effect;
        public ParticleEffect Effect
        {
            get => _effect;
            set
            {
                TickFlags  |= UpdateFlags.ParticleEffect;
                _effect = value;
            }
        }

        private Animation _animData;
        public Animation Animation
        {
            get => _animData;
            set
            {
                TickFlags |= UpdateFlags.Animation;
                _animData = value;
            }
        }

        private IWorldEntity _interactingEntity;

        public IWorldEntity InteractingEntity
        {
            get => _interactingEntity;
            set
            {
                _interactingEntity = value;
                TickFlags |= UpdateFlags.InteractingEntity;
            }
        }

        private (ushort x, ushort y)? _facingCoordinate;

        [CanBeNull]
        public (ushort x, ushort y)? FacingCoordinate
        {
            get => _facingCoordinate;
            set
            {
                _facingCoordinate = value;
                if (value != null)
                    TickFlags |= UpdateFlags.FacingCoordinate;
            }
        }

        [CanBeNull] public string LastSentTextMessage { get; private set; }

        public bool NeedsPositionInit { get; private set; } = true;
        #endregion

        public int ViewRange { get; set; } = 15;
        public MovementController Movement { get; }

        private short _definition;

        public short NpcDefinitionId
        {
            get => _definition;
            set
            {
                if (_definition == value) return;

                _definition = value;
                TickFlags |= UpdateFlags.Definition;
            }
        }
        public short UniqueNpcId { get; }

        public Npc(IServiceProvider service,
            short npcDefId,
            IPosition pos,
            PlaneOfExistance poe = null) : this(service, npcDefId, pos.X, pos.Y, pos.Z, poe) { }

        public Npc(IServiceProvider service,
            short npcDefId,
            int x, int y, byte z,
            PlaneOfExistance poe = null) : base(service)
        {
            if (0 > npcDefId) throw new ArgumentOutOfRangeException(nameof(npcDefId));

            Transform = new ServerTransform(this, x, y, z, poe);
            Movement = new MovementController(service, this);

            _definition = npcDefId;
            UniqueNpcId = IdPool.NextNpc();

            // queue for immediate update
            service.ThrowOrGet<IMainLoop>().Npc.Enqueue(this);

            // register
            Server.Npcs.Register(this);
        }

        /// <summary>
        /// Forces the entity to say, above their head overhead, the given text.
        /// </summary>
        public void Say(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            LastSentTextMessage = text;
            TickFlags |= UpdateFlags.Text;
        }

        protected override void InternalDestroy()
        {
            IdPool.FreeNpc(UniqueNpcId);
            Server.Npcs.Unregister(this);
        }

        public void OnMoved()
        {
            // ignored, todo : reset npc facing coordinate?
        }

        public override void Update(IMainLoop loop)
        {
            // reset update flags
            NeedsPositionInit = false;
            TickFlags = 0;
            LastSentTextMessage = null;

            EntityHelper.TryResetInteractingEntity(this);

            // todo : destroy if 0 > health

            loop.Npc.Enqueue(this);
        }

        public override bool CanSee(IWorldEntity ent)
        {
            if (ent.IsDestroyed)
                return false;

            if (ent.Transform.Z != Transform.Z)
                return false;

            if (!Transform.PoE.ContainsEntity(ent))
                return false;

            return ent.Transform.MaxDistanceTo(Transform) <= ViewRange;
        }

        public override string ToString() => $"Npc def-id {NpcDefinitionId} (UEI: {UniqueNpcId} UNI {UniqueNpcId})";
    }
}