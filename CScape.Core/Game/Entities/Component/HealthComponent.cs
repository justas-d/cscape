using System.Collections.Generic;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Interface;
using CScape.Core.Network;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    public interface IGameInterface
    {
        int Id { get; }

        IPacket GetShowPacket();
        IPacket GetClosePacket();
        IEnumerable<IPacket> GetUpdatePackets();

        void ReceiveMessage(EntityMessage msg);
    }


    public struct InterfaceMetadata
    {
        public InterfaceType Type { get; }

        [NotNull]
        public IGameInterface Interface { get; }

        public int Index { get;}

        private InterfaceMetadata(InterfaceType type, IGameInterface interf, int index)
        {
            Type = type;
            Interface = interf;
            Index = index;
        }

        public static InterfaceMetadata Main(IGameInterface i) 
            => new InterfaceMetadata(InterfaceType.Main, i, -1);

        public static InterfaceMetadata Sidebar(IGameInterface i, int index)
            => new InterfaceMetadata(InterfaceType.Sidebar, i, index);

        public static InterfaceMetadata Chat(IGameInterface i)
            => new InterfaceMetadata(InterfaceType.Chat, i, -1);

        public static InterfaceMetadata Input(IGameInterface i)
            => new InterfaceMetadata(InterfaceType.Input, i, -1);
    }


    public sealed class InterfaceComponent : EntityComponent
    {
        public const int MaxSidebarInterfaces = 15;

        public override int Priority { get; }

        private readonly Dictionary<int, IGameInterface> _interfaces 
            = new Dictionary<int, IGameInterface>();

        private readonly IGameInterface[] _sidebars = new IGameInterface[MaxSidebarInterfaces];

        [CanBeNull]
        public IGameInterface Main { get; }
        [CanBeNull]
        public IGameInterface Chat { get; }
        [CanBeNull]
        public IGameInterface Input { get; }

        [NotNull]
        public IList<IGameInterface> Sidebar => _sidebars;

        [NotNull]
        public IReadOnlyDictionary<int, IGameInterface> All => _interfaces;

        public readonly HashSet<int> _interfaceIdsInQueue = new HashSet<int>();
        private readonly Queue<InterfaceMetadata> _queue = new Queue<InterfaceMetadata>();

        public InterfaceComponent([NotNull] Entity parent) : base(parent)
        {
        }

        public void Close(int id)
        {
            // TODO remember to remove id from  _interfaceIdsInQueue
        }

        public void Show(InterfaceMetadata meta)
        {
            // do not allow duplicate interfaces to be queued up
            if (_interfaceIdsInQueue.Contains(meta.Interface.Id))
                return;

            
            _interfaceIdsInQueue.Add(meta.Interface.Id);

        }
        
        public override void ReceiveMessage(EntityMessage msg)
        {
            throw new System.NotImplementedException();
        }
    }


    public sealed class HealthComponent : EntityComponent
    {
        private int _health;
        private int _maxHealth;

        public override int Priority { get; }

        private int Health
        {
            get => _health;
            set
            {
                _health = value;
                CheckForDeath();
            }
        }

        private int MaxHealth
        {
            get => _maxHealth;
            set
            {
                _maxHealth = value;
                CheckForDeath();
            }
        }

        public HealthComponent(Entity parent, int maxHealth = 1, int health = 1)
            :base(parent)
        {
            MaxHealth = maxHealth;
            Health = health;
        }
        
        private void CheckForDeath()
        {
            if (0 >= Health)
            {
                Parent.SendMessage(
                    new EntityMessage(
                        this,
                        EntityMessage.EventType.JustDied,
                        null));
            }
        }
        
        public override void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.TookDamage:
                {
                    var dmg = msg.AsTookDamage();
                    Health -= dmg.Damage;
                    break;
                }
                case EntityMessage.EventType.HealedHealth:
                {
                    var hp = msg.AsHealedHealth();
                    Health += hp;
                    break;
                }
            }
        }
    }
}