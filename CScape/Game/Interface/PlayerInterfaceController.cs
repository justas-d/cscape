using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CScape.Game.Entity;
using CScape.Network;
using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    public class PlayerInterfaceController : IInterfaceController, IInterfaceLifetimeManager
    {
        public Player Player { get; }

        private readonly Dictionary<int, IManagedInterface> _openInterfaces = new Dictionary<int, IManagedInterface>();
        private int _mainIdx;
        private int _inputIdx;

        public IInterfaceApi Main => _openInterfaces.ge

        /*
        private IManagedInterface _main;
        private IManagedInterface _input;
        private ImmutableDictionary<int, IInterfaceApi> _sidebar = ImmutableDictionary<int, IInterfaceApi>.Empty;

        public IInterfaceApi Main => _main;
        public IReadOnlyDictionary<int, IInterfaceApi> Sidebar => _sidebar;
        public IInterfaceApi Input => _input;
        */

        private ImmutableList<IPacket> _packetBacklog = ImmutableList<IPacket>.Empty;

        public PlayerInterfaceController([NotNull] Player player)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public bool TryShow(IManagedInterface interf)
        {
            bool TryShowProceedure(ref IManagedInterface setField)
            {
                if (setField != null)
                    return false;

                if (!interf.TryShow(this))
                    return false;

                setField = interf;
                return true;
            }

            switch (interf.Info.Type)
            {
                case InterfaceInfo.InterfaceType.Main:
                    return TryShowProceedure(ref _main);

                case InterfaceInfo.InterfaceType.Input:
                    return TryShowProceedure(ref _input);

                case InterfaceInfo.InterfaceType.Sidebar:
                    if (Sidebar.ContainsKey(interf.Info.SidebarIndex))
                        return false;

                    if (!interf.TryShow(this))
                        return false;

                    _sidebar = _sidebar.Add(interf.Info.SidebarIndex, interf);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IEnumerable<IPacket> GetUpdates()
        {
            var ret = _packetBacklog;
            _packetBacklog = ImmutableList<IPacket>.Empty;

            ret.AddRange()
        }

        public void HandleButton(int interfaceId, int buttonId)
        {
            throw new NotImplementedException();
        }

        void IInterfaceLifetimeManager.Close(IManagedInterface interf)
        {
            switch (interf.Info.Type)
            {
                case InterfaceInfo.InterfaceType.Main:
                    _main = null;
                    break;
                case InterfaceInfo.InterfaceType.Input:
                    _input = null;
                    break;
                case InterfaceInfo.InterfaceType.Sidebar:
                    _sidebar = _sidebar.Remove(interf.Info.SidebarIndex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}