using System;
using System.Collections.Generic;
using System.Diagnostics;
using CScape.Core.Game.Entities.Interface;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    public sealed class InterfaceComponent : EntityComponent
    {
        public const int MaxSidebarInterfaces = 15;

        public override int Priority { get; }

        private readonly Dictionary<int, InterfaceMetadata> _interfaces 
            = new Dictionary<int, InterfaceMetadata>();

        private readonly IGameInterface[] _sidebars = new IGameInterface[MaxSidebarInterfaces];

        [CanBeNull]
        public IGameInterface Main { get; private set; }
        [CanBeNull]
        public IGameInterface Chat { get; private set; }
        [CanBeNull]
        public IGameInterface Input { get; private set; }

        [NotNull]
        public IList<IGameInterface> Sidebar => _sidebars;

        [NotNull]
        public IReadOnlyDictionary<int, InterfaceMetadata> All => _interfaces;

        private readonly HashSet<int> _interfaceIdsInQueue = new HashSet<int>();
        private readonly HashSet<int> _pressedButtonIds = new HashSet<int>();
        private readonly List<InterfaceMetadata> _queue = new List<InterfaceMetadata>();

        public InterfaceComponent([NotNull] Entity parent) : base(parent)
        {
        }

        public void Close(int id)
        {
            Debug.Assert(_interfaces.ContainsKey(id));

            var meta = _interfaces[id];

            switch (meta.Type)
            {
                case InterfaceType.Main:
                    Main = null;
                    break;

                case InterfaceType.Sidebar:
                    _sidebars[meta.Index] = null;
                    break;

                case InterfaceType.Chat:
                    Chat = null;
                    break;

                case InterfaceType.Input:
                    Input = null;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            Parent.SendMessage(
                new GameMessage(
                    this, GameMessage.Type.InterfaceClosed, meta));
        }

        public void Show(InterfaceMetadata meta)
        {
            // make sure sidebar idx is in range
            if (meta.Type == InterfaceType.Sidebar)
            {
                if (!meta.Index.InRange(0, MaxSidebarInterfaces))
                    return;
            }

            /* multiple checks to not allow duplicate interfaces to be queued up */
            if (_interfaceIdsInQueue.Contains(meta.Interface.Id))
                return;

            if (_interfaces.ContainsKey(meta.Interface.Id))
                return;
            
            _interfaceIdsInQueue.Add(meta.Interface.Id);
            _queue.Add(meta);
        }

        public bool CanShow(InterfaceMetadata meta)
        {
            switch (meta.Type)
            {
                case InterfaceType.Main:
                    return Main == null;
                    
                case InterfaceType.Sidebar:
                    if (!meta.Index.InRange(0, MaxSidebarInterfaces)) return false;
                    return _sidebars[meta.Index] == null;
                    
                case InterfaceType.Chat:
                    return Chat == null;
                    
                case InterfaceType.Input:
                    return Input == null;
                    
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        /// <summary>
        /// Does no checks to see if the interface can be shown.
        /// Registers an interface as shown, adds it to the all list and makes it "alive" 
        /// immediatelly
        /// </summary>
        private void InternalShow(InterfaceMetadata meta)
        {
            _interfaces.Add(meta.Index, meta);

            switch (meta.Type)
            {
                case InterfaceType.Main:
                    Main = meta.Interface;

                    break;
                case InterfaceType.Sidebar:
                    _sidebars[meta.Index] = meta.Interface;
                    
                    break;
                case InterfaceType.Chat:
                    Chat = meta.Interface;

                    break;
                case InterfaceType.Input:
                    Input = meta.Interface;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Parent.SendMessage(
                new GameMessage(
                    this, GameMessage.Type.NewInterfaceShown, meta));
        }

        private void Update()
        {
            // go through the queue in a manner which allows us to remove entries from it
            // as we are interating it
            for (var i = 0; i < _queue.Count; )
            {
                var meta = _queue[i];
                if (CanShow(meta))
                {
                   
                    // if we can show the interface, remove from queue
                    _interfaceIdsInQueue.Remove(meta.Interface.Id);
                    _queue.RemoveAt(i);

                    InternalShow(meta);
                }
                else
                    ++i;
            }

            _pressedButtonIds.Clear();
        }

        private void OnActionOccurred()
        {
            void CloseNotNull(IGameInterface interf) { if(interf != null) Close(interf.Id); } 

            CloseNotNull(Input);
            CloseNotNull(Chat);
            CloseNotNull(Main);

            _queue.Clear();
            _interfaceIdsInQueue.Clear();
        }

        private void PropogateMsgToInterfaces(GameMessage msg)
        {
            foreach (var interf in _interfaces.Values)
            {
                interf.Interface.ReceiveMessage(msg);
            }
        }

        public override void ReceiveMessage(GameMessage msg)
        {
            switch (msg.Event)
            {
                case GameMessage.Type.ForcedMovement:
                case GameMessage.Type.HealedHealth:
                case GameMessage.Type.TookDamage:
                case GameMessage.Type.JustDied:
                case GameMessage.Type.Move:
                case GameMessage.Type.Teleport:
                    OnActionOccurred();
                    break;

                case GameMessage.Type.FrameUpdate:
                    Update();
                    break;

                case GameMessage.Type.ButtonClicked:
                {
                    /*
                     * we handle button click data separately in order to filter out duplicate
                     * clicks during a frame.
                     * 
                     * We want to ensure that if a button id is pressed, we only send out one
                     * event that signal that during that frame.
                     */

                    var data = msg.AsButtonClicked();

                    if (_pressedButtonIds.Add(data.ButtonId))
                        PropogateMsgToInterfaces(msg);
                        
                    break;
                }
            }

            // we handle ButtonClicked events separately in OnButtonClicked
            if (msg.Event != GameMessage.Type.ButtonClicked)
                PropogateMsgToInterfaces(msg);
        }
    }
}