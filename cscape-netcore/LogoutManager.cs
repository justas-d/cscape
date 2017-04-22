using System.Collections.Generic;
using CScape.Game.Entity;
using CScape.Network.Sync;

namespace CScape
{
    public static class LogoutManager
    {
        public static void PreLogout(Player player)
        {
            player.PoE.RemoveEntity(player); // remove from world
            LogoffPacket.Static.Send(player.Connection.OutStream); // write logoff message
        }

        /// <summary>
        /// Should be called after the logoffpacket is sent.
        /// </summary>
        public static void PostLogout(Player player, Queue<uint> removeQueue)
        {
            player.Connection.Dispose(); // shut down the connection
            player.Save(); // queue save

            // queue the player for removal from playing list, since they cleanly logged out.
            if(player.LogoutMethod == Player.LogoutType.Clean)
                removeQueue.Enqueue(player.UniqueEntityId);
        }
    }
}