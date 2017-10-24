using System;
using System.Diagnostics;
using CScape.Core.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    //todo: change username feature
    //todo: change password feature

    /// <summary>
    /// Defines a player entity that exists in the world.
    /// </summary>
    public sealed class Player 
        : WorldEntity, IMovingEntity, IObserver, IDamageable, IEquatable<Player>
    {


        [NotNull] private readonly IPlayerModel _model;



        public bool TeleportToDestWhenWalking { get; set; }


        public Player([NotNull] IPlayerModel model, ISocketContext socket,
            [NotNull] IServiceProvider services, bool isHighDetail) : base(services)
        {



            // container interfaces
            var res = Interfaces.TryShow(new ItemSidebarInterface(ids.BackpackSidebar, ids.BackpackSidebarIdx, Inventory,null));
            Debug.Assert(res, "Cannot show container interface in player ctor ");
            res = Interfaces.TryShow(new ItemSidebarInterface(ids.EquipmentSidebar, ids.EquipmentSidebarIdx, Equipment, null));
            Debug.Assert(res, "Cannot show container interface in player ctor ");

        }


        public override void Update(IMainLoop loop)
        {
            // sync db model
            _model.X = Transform.X;
            _model.Y = Transform.Y;
            _model.Z = Transform.Z;

        }
    }
}