using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    public class PlayerInterfaceController : IInterfaceController, IInterfaceLifetimeManager
    {
        public Player Player { get; }

        private IInterface _main;
        private IInterface _input;
        private ImmutableDictionary<int, IInterface> _sidebar = ImmutableDictionary<int, IInterface>.Empty;

        public IInterface Main => _main;
        public IReadOnlyDictionary<int, IInterface> Sidebar => _sidebar;
        public IInterface Input => _input;

        public PlayerInterfaceController([NotNull] Player player)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public bool TryShow(IManagedInterface interf)
        {
            bool TryShowProceedure(ref IInterface setField)
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