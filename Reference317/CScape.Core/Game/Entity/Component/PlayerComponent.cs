using System;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Game.Interface;
using CScape.Core.Game.Item;
using CScape.Core.Json.Model;
using CScape.Core.Network.Packet;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Interface;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Core.Game.Entity.Component
{
    [RequiresComponent(typeof(PlayerInventoryComponent))]
    [RequiresComponent(typeof(HealthComponent))]
    public sealed class PlayerComponent : EntityComponent, IPlayerComponent
    {
        public enum Title : byte
        {
            Normal = 0,
            Moderator = 1,
            Admin = 2
        }

        public const int MaxUsernameChars = 12;
        public const int MaxPasswordChars = 64;

        public int InstanceId { get; }

        public PlayerAppearance Apperance { get; private set; }
        
        public int TitleId { get; set; }
        public bool IsMember { get; }

        [NotNull]
        public string Username { get; }

        public override int Priority => (int)ComponentPriority.Player;

        [CanBeNull] private readonly Action<PlayerComponent> _destroyCallback;

        public PlayerComponent(
            [NotNull] IEntity parent,
            [NotNull] string username,
            PlayerAppearance appearance,
            bool isMember,
            int titleId,
            int instanceId,
            [CanBeNull] Action<PlayerComponent> destroyCallback)
            :base(parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("message", nameof(username));
            }

            _destroyCallback = destroyCallback ?? throw new ArgumentNullException(nameof(destroyCallback));
            InstanceId = instanceId;
            TitleId = titleId;
            IsMember = isMember;
            Username = username;

            SetAppearance(appearance);
        }

        public void SetAppearance(PlayerAppearance appearance)
        {
            Apperance = appearance;
            Parent.SendMessage(new PlayerAppearanceMessage(Username, appearance, Parent.AssertGetPlayerContainers().Equipment));
        }

        private void InitPlayer()
        {
            var net = Parent.GetNetwork();
            if (net != null)
            {
                net.SendPacket(new InitializePlayerPacket(InstanceId, IsMember));
                // TODO : delegate the retrieval of SetPlayerOptionPacket to the current Region
                net.SendPacket(SetPlayerOptionPacket.Follow);
                net.SendPacket(SetPlayerOptionPacket.TradeWith);
                net.SendPacket(SetPlayerOptionPacket.Report);
            }

            var interf = Parent.GetInterfaces();
            if (interf != null)
            {
                var db = Parent.Server.Services.GetService<IInterfaceIdDatabase>();
                if (db != null)
                {
                    interf.ShowSidebar(new UnimplementedSidebarInterface(db.SkillSidebar, db.SkillSidebarIdx), db.SkillSidebarIdx);
                    interf.ShowSidebar(new UnimplementedSidebarInterface(db.QuestSidebar, db.QuestSidebarIdx), db.QuestSidebarIdx);
                    interf.ShowSidebar(new UnimplementedSidebarInterface(db.PrayerSidebar, db.PrayerSidebarIdx), db.PrayerSidebarIdx);
                    // todo : different spellbooks
                    interf.ShowSidebar(new UnimplementedSidebarInterface(db.StandardSpellbookSidebar, db.SpellbookSidebarIdx), db.SpellbookSidebarIdx);
                    interf.ShowSidebar(new UnimplementedSidebarInterface(db.FriendsListSidebar, db.FriendsSidebarIdx), db.FriendsSidebarIdx);
                    interf.ShowSidebar(new UnimplementedSidebarInterface(db.IgnoreListSidebar, db.IgnoresSidebarIdx), db.IgnoresSidebarIdx);
                    interf.ShowSidebar(new UnimplementedSidebarInterface(db.LogoutSidebar, db.LogoutSidebarIdx), db.LogoutSidebarIdx);
                    // todo : high and low detail
                    interf.ShowSidebar(new UnimplementedSidebarInterface(db.OptionsHighDetailSidebar, db.OptionsSidebarIdx), db.OptionsSidebarIdx);
                    interf.ShowSidebar(new UnimplementedSidebarInterface(db.ControlsSidebar, db.ControlsSidebarIdx), db.ControlsSidebarIdx);

                    interf.ShowSidebar(new UnimplementedSidebarInterface(db.BackpackSidebar, db.BackpackSidebarIdx), db.BackpackSidebarIdx);
                    interf.ShowSidebar(new UnimplementedSidebarInterface(db.EquipmentSidebar, db.EquipmentSidebarIdx), db.EquipmentSidebarIdx);

                    var items = Parent.AssertGetPlayerContainers();

                    interf.Show(InterfaceMetadata.Register(new InventoryInterface(db.BackpackContainer, items.Backpack)));
                    interf.Show(InterfaceMetadata.Register(new InventoryInterface(db.EquipmentContainer, items.Equipment)));
                }
                else
                {
                    Parent.SystemMessage("Cannot intialize player sidebars due to there not being a InterfaceIdDatabase in the server's IServerProvider.",
                        CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Entity);
                }
            }
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.QueuedForDeath:
                {
                    _destroyCallback?.Invoke(this);
                    break;
                }

                case (int) MessageId.JustDied:
                {
                    // TODO : handle death in PlayerComponent
                    break;
                }
                case (int) MessageId.EquipmentChange:
                {
                    Parent.SendMessage(new PlayerAppearanceMessage(
                        Username,
                        Apperance,
                        Parent.AssertGetPlayerContainers().Equipment));
                    break;
                }
                case (int) MessageId.Initialize:
                {
                    InitPlayer();
                    break;
                }
            }
        }

        /// <summary>
        /// Logs out (destroys) the entity only if it's safe for the player to log out.
        /// </summary>
        /// <returns>True - the player logged out, false otherwise</returns>
        public bool TryLogout()
        {
            // TODO : check if the player can log out. (in combat or something)

            Parent.Handle.Destroy();
            return true;
        }

        /// <summary>
        /// Forcefully drops the connection. 
        /// Keeps the player alive in the world.
        /// Should only be used when something goes wrong.
        /// </summary>
        public void ForcedLogout()
        {
            var net = Parent.GetNetwork();
            if (net != null)
                net.DropConnection();
            else
                Parent.Handle.Destroy();
        }

        public bool Equals(IPlayerComponent other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Username);
        }

        public bool Equals(string other)
        {
            return string.Equals(Username, other, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is IPlayerComponent && Equals((IPlayerComponent)obj);
        }

        public override string ToString()
        {
            return $"Player \"{Username}\" PID: {InstanceId})";
        }

        public override int GetHashCode()
        {
            return Username.GetHashCode() * 31;
        }
    }
}
