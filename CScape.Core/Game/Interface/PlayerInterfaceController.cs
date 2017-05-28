using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using CScape.Core.Network;
using JetBrains.Annotations;

namespace CScape.Core.Game.Interface
{
    public class PlayerInterfaceController : IInterfaceManager
    {
        public Player Player { get; }
        private ILogger Log => Player.Log;

        private class Backend : IInterfaceManagerApiBackend
        {
            private readonly PlayerInterfaceController _frontEnd;

            public const int MaxSidebarInterfaces = 15;

            private readonly List<IShowableInterface> _sidebar = new List<IShowableInterface>(MaxSidebarInterfaces);
            private readonly Dictionary<int, IBaseInterface> _all = new Dictionary<int, IBaseInterface>();

            public IShowableInterface Main { get; set; }

            public IShowableInterface Chat { get; set; }
            public IShowableInterface Input { get; set; }

            public IReadOnlyList<IShowableInterface> PublicSidebar => _sidebar;
            public IList<IShowableInterface> Sidebar => _sidebar;

            public IReadOnlyDictionary<int, IBaseInterface> PublicAll => _all;
            public IDictionary<int, IBaseInterface> All => _all;
            public IInterfaceManager Frontend => _frontEnd;

            public Backend(PlayerInterfaceController frontEnd)
            {
                _frontEnd = frontEnd;
                _sidebar.AddRange(Enumerable.Repeat(default(IShowableInterface), MaxSidebarInterfaces));
            }

            public ImmutableList<IEnumerable<IPacket>> UpdBacklog { get; set; } =
                ImmutableList<IEnumerable<IPacket>>.Empty;

            public void NotifyOfRegister(IApiInterface interf)
            {
                All.Add(interf.Id, interf);
            }

            public void NotifyOfClose(IApiInterface interf)
            {
                // dump updates
                UpdBacklog = UpdBacklog.Add(interf.GetUpdates());

                // unregister
                _frontEnd.TryUnregister(interf.Id);
            }
        }

        public IShowableInterface Main => _backend.Main;
        public IShowableInterface Chat => _backend.Chat;
        public IShowableInterface Input => _backend.Input;

        public IReadOnlyList<IShowableInterface> Sidebar => _backend.PublicSidebar;
        public IReadOnlyDictionary<int, IBaseInterface> All => _backend.PublicAll;

        private readonly Backend _backend;

        public PlayerInterfaceController([NotNull] Player player)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            _backend = new Backend(this);
        }

        public IBaseInterface TryGetById(int id) => !All.ContainsKey(id) ? null : All[id];

        public bool TryShow<T>(T interf) where T : IApiInterface, IShowableInterface
        {
            if (!TryRegister(interf))
                return false;

            interf.Show();
            return true;
        }

        public bool TryRegister(IApiInterface interf)
        {
            if (interf.IsRegistered)
            {
                Log.Warning(this, $"Tried to register already registered interface ({interf.Id})");
                return false;
            }

            return interf.TryRegisterApi(_backend);
        }

        public bool TryUnregister(int id)
        {
            if (!All.ContainsKey(id))
                return false;

            var apiInterface = All[id] as IApiInterface;

            _backend.All.Remove(id);
            apiInterface?.UnregisterApi();
            return true;
        }

        public bool CanShow(InterfaceType type, int? sidebarSlotIndex)
        {
            switch (type)
            {
                case InterfaceType.Main: return Main == null;
                case InterfaceType.Sidebar:

                    if (sidebarSlotIndex == null)
                    {
                        Log.Warning(this, "Passed null sidebar idx to PlayerInterfaceController.CanShow");
                        return false;
                    }

                    var idx = sidebarSlotIndex.Value;
                    if (0 > idx || idx >= Backend.MaxSidebarInterfaces)
                    {
                        Log.Warning(this,
                            $"Sidebar idx ({idx}) passed to PlayerInterfaceController.CanShow is out of range 0 < idx < {Backend.MaxSidebarInterfaces}");
                        return false;
                    }

                    return Sidebar[idx] == null;

                case InterfaceType.Chat: return Chat == null;
                case InterfaceType.Input: return Input == null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public HashSet<int> PressedButtonIds { get; } = new HashSet<int>();

        public void HandleButton(Player player, int interfaceId, int buttonId)
        {
            if (!All.ContainsKey(interfaceId))
            {
                Log.Warning(this, $"Pressed button ({buttonId}) on unregistered interface ({interfaceId})");
                return;
            }

            // combat button spamming by keeping track what buttons we've pressed.
            if (!PressedButtonIds.Add(buttonId))
                return;

            player.DebugMsg($"Button {buttonId} interface {interfaceId} ", ref player.DebugInterface);

            var interf = All[interfaceId] as IShowableInterface;
            interf?.ButtonHandler?.OnButtonPressed(player, buttonId);
        }

        public void OnActionOccurred()
        {
            Input?.TryClose();
            Chat?.TryClose();
        }

        public IEnumerable<IPacket> GetUpdates()
        {
            var backlog = _backend.UpdBacklog;
            _backend.UpdBacklog = ImmutableList<IEnumerable<IPacket>>.Empty;

            return backlog.SelectMany(i => i) // backlog
                .Concat(All.Values.SelectMany(i => i.GetUpdates())); //+  active
        }
    }
}