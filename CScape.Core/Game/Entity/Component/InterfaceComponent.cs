using System;
using System.Collections.Generic;
using System.Diagnostics;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Interface;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    public sealed class InterfaceComponent : EntityComponent, IInterfaceComponent
    {
        public const int MaxSidebarInterfaces = 15;

        public override int Priority => (int)ComponentPriority.InterfaceComponent;

        private readonly Dictionary<int, InterfaceMetadata> _interfaces 
            = new Dictionary<int, InterfaceMetadata>();

        private readonly IGameInterface[] _sidebars = new IGameInterface[MaxSidebarInterfaces];

        public IGameInterface Main { get; private set; }
        public IGameInterface Chat { get; private set; }
        public IGameInterface Input { get; private set; }
        public IList<IGameInterface> Sidebar => _sidebars;
        public IReadOnlyDictionary<int, InterfaceMetadata> All => _interfaces;

        private readonly HashSet<int> _interfaceIdsInQueue = new HashSet<int>();
        private readonly HashSet<int> _pressedButtonIds = new HashSet<int>();
        private readonly List<InterfaceMetadata> _queue = new List<InterfaceMetadata>();

        public InterfaceComponent([NotNull] IEntity parent) : base(parent)
        {
        }

        public bool Close(int id)
        {
            if (!_interfaces.ContainsKey(id))
                return false;
                
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

                case InterfaceType.None: break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Debug.Assert(_interfaces.Remove(id));
            meta.Interface.CloseForEntity(Parent);

            return true;
        }

        public bool Show(InterfaceMetadata meta)
        {
            // make sure sidebar idx is in range
            if (meta.Type == InterfaceType.Sidebar)
            {
                if (!meta.Index.InRange(0, MaxSidebarInterfaces))
                    return false;
            }

            /* multiple checks to not allow duplicate interfaces to be queued up */
            if (_interfaceIdsInQueue.Contains(meta.Interface.Id))
                return false;

            if (_interfaces.ContainsKey(meta.Interface.Id))
                return false;
            
            _interfaceIdsInQueue.Add(meta.Interface.Id);
            _queue.Add(meta);

            return true;
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

                case InterfaceType.None:
                    return !_interfaces.ContainsKey(meta.Interface.Id);
                    
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
            _interfaces.Add(meta.Interface.Id, meta);

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
                case InterfaceType.None:
                    _interfaces[meta.Interface.Id] = meta;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _interfaces[meta.Interface.Id] = meta;

            meta.Interface.ShowForEntity(Parent);
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
            
            // update interfaces
            foreach (var interf in All)
                interf.Value.Interface.UpdateForEntity(Parent);
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

        private void PropogateMsgToInterfaces(IGameMessage msg)
        {
            foreach (var interf in _interfaces.Values)
            {
                interf.Interface.ReceiveMessage(Parent, msg);
            }
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.ForcedMovement:
                case (int)MessageId.HealthChanged:
                case (int)MessageId.TookDamageLostHealth:
                case (int)MessageId.EatHealedHealth:
                case (int)MessageId.JustDied:
                case (int)MessageId.Move:
                case (int)MessageId.Teleport:
                    OnActionOccurred();
                    break;

                case (int)MessageId.FrameBegin:
                    Update();
                    break;

                case (int)MessageId.ButtonClicked:
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
            if (msg.EventId != (int)MessageId.ButtonClicked)
                PropogateMsgToInterfaces(msg);
        }
    }
}