using System;
using System.Collections.Generic;
using CScape.Core;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using CScape.Core.Game.World;
using CScape.Core.Injection;
using CScape.Core.Network.Packet;

namespace CScape.Basic.Commands
{
    [CommandsClass]
    public sealed class TestCommandClass
    {
        private PlaneOfExistance _diffPoe;

        [CommandMethod("npcanim")]
        public void SetNpcAnim(CommandContext ctx)
        {
            var uni = 0;
            short animId = 0;
            byte delay = 0;

            if (!ctx.Read(b =>
            {
                b.ReadNumber("UniqueNpcId", ref uni);
                b.ReadNumber("animation id", ref animId);
                b.ReadNumber("delay", ref delay);
            })) return;

            var npc = ctx.Callee.Server.Npcs.GetById(uni);
            if (npc == null)
            {
                ctx.Callee.SendSystemChatMessage("Npc not found.");
                return;
            }

            npc.AnimationData = (animId, delay);
        }

        [CommandMethod("npc")]
        public void SpawnNpc(CommandContext ctx)
        {
            short defId = 0;
            if (!ctx.Read(b => b.ReadNumber("definition id", ref defId))) return;

            var npc = new Npc(ctx.Callee.Server.Services, defId, ctx.Callee.Transform);
            npc.Movement.Directions = new FollowDirectionProvider(npc, ctx.Callee);
        }

        [CommandMethod("clearinv")]
        public void ClearInv(CommandContext ctx)
        {
            for (var i = 0; i < ctx.Callee.Inventory.Size; i++)
                ctx.Callee.Inventory.ExecuteChangeInfo(ItemProviderChangeInfo.Remove(i));
        }

        [CommandMethod("rngitem")]
        public void RandomItems(CommandContext ctx)
        {
            const int max = 5000;
            var count = 0;
            var rng = new Random();
            var used = new HashSet<int>();

            if (!ctx.Read(b =>
            {
                b.ReadNumber("count", ref count);
            })) return;

            for (var i = 0; i < count; i++)
            {
                int id;
                do
                {
                    id = rng.Next(0, max);
                } while (used.Contains(id));
                used.Add(id);

                ctx.Callee.Inventory.Items.ExecuteChangeInfo(ctx.Callee.Inventory.Items.CalcChangeInfo(id, rng.Next(1, int.MaxValue)));
            }
        }

        [CommandMethod("close")]
        public void CloseServer(CommandContext ctx)
        {
            ctx.Callee.Server.Dispose();
        }

        [CommandMethod("item")]
        public void GiveItem(CommandContext ctx)
        {
            int id = 0;
            int amount = 1;

            if (!ctx.Read(b =>
            {
                b.ReadNumber("id", ref id);
                b.ReadNumber("amount", ref amount, true);
            })) return;

            var change = ctx.Callee.Inventory.Items.CalcChangeInfo(id, amount);
            ctx.Callee.Inventory.Items.ExecuteChangeInfo(change);

            ctx.Callee.SendSystemChatMessage($"Giving {amount} with overflow {change.OverflowAmount}");
        }

        [CommandMethod("setitem")]
        public void SetItem(CommandContext ctx)
        {
            var idx = 0;
            var id = 0;
            var amount = 1;

            if (!ctx.Read(b =>
            {
                b.ReadNumber("index", ref idx);
                b.ReadNumber("id", ref id);
                b.ReadNumber("amount", ref amount, true);
            })) return;

            ctx.Callee.Inventory.Items.Provider.SetId(idx, id);
            ctx.Callee.Inventory.Items.Provider.SetAmount(idx, amount);
        }

        [CommandMethod("test soi")]
        public void TestShowItemOnInterfacePacket(CommandContext ctx)
        {
            int iid = 0;
            int zoom = 0;
            int itemId = 0;

            if (!ctx.Read(b =>
            {
                b.ReadNumber("interface id", ref iid);
                b.ReadNumber("zoom", ref zoom);
                b.ReadNumber("item id", ref itemId);
            })) return;

            ctx.Callee.Connection.SendPacket(new ShowItemOnInterfacePacket(iid, zoom, itemId));
        }

        [CommandMethod("poe now")]
        public void PrintPoe(CommandContext ctx)
        {
            ctx.Callee.SendSystemChatMessage(ctx.Callee.Transform.PoE.ToString());
        }

        [CommandMethod("poe test")]
        public void SwitchPoe(CommandContext ctx)
        {
            if (_diffPoe == null)
                _diffPoe = new PlaneOfExistance(ctx.Callee.Server, "test_poe");

            ctx.Callee.Transform.SwitchPoE(_diffPoe);
        }

        [CommandMethod("ow")]
        public void PoeOverworld(CommandContext ctx)
        {
            ctx.Callee.Transform.SwitchPoE(ctx.Callee.Server.Overworld);
        }

        [CommandMethod("tickrate")]
        public void SetTickRate(CommandContext ctx)
        {
            var tickrate = 0;
            if (!ctx.Read(p => p.ReadNumber("tickrate", ref tickrate))) return;
            ctx.Callee.Server.Services.ThrowOrGet<IMainLoop>().TickRate = tickrate;
        }

        [CommandMethod("debug stats")]
        public void ToggleDebugStats(CommandContext ctx)
        {
            ctx.Callee.DebugStats = !ctx.Callee.DebugStats;
            ctx.Callee.SendSystemChatMessage("Toggling stat debug.");
        }

        [CommandMethod("debug packet")]
        public void ToggleDebugPackets(CommandContext ctx)
        {
            ctx.Callee.DebugPackets = !ctx.Callee.DebugPackets;
            ctx.Callee.SendSystemChatMessage("Toggling packet debug.");
        }

        [CommandMethod("debug cmd")]
        public void ToggleDebugCmd(CommandContext ctx)
        {
            ctx.Callee.DebugCommands = !ctx.Callee.DebugCommands;
            ctx.Callee.SendSystemChatMessage("Toggling command debug.");
        }

        [CommandMethod("walk tp")]
        public void ToggleWalkTp(CommandContext ctx)
        {
            ctx.Callee.TeleportToDestWhenWalking = !ctx.Callee.TeleportToDestWhenWalking;
            ctx.Callee.SendSystemChatMessage("Toggling teleport on walk.");
        }

        [CommandMethod]
        public void Run(CommandContext ctx)
        {
            ctx.Callee.Movement.IsRunning = !ctx.Callee.Movement.IsRunning;
        }

        [CommandMethod("data")]
        public void DataCallback(CommandContext ctx)
        {
            ctx.Callee.SendSystemChatMessage($"\"{ctx.Data}\"");
        }

        [CommandMethod("pos get")]
        public void GetPos(CommandContext ctx)
        {
            var player = ctx.Callee;
            player.SendSystemChatMessage($"X: {player.Transform.X} Y: {player.Transform.Y} Z: {player.Transform.Z}");
            player.SendSystemChatMessage($"LX: {player.ClientTransform.Local.x} LY: {player.ClientTransform.Local.y}");
            player.SendSystemChatMessage($"CRX: {player.ClientTransform.ClientRegion.x} + 6 CRY: {player.ClientTransform.ClientRegion.y} + 6");
        }

        [CommandMethod("pos set")]
        public void SetPos(CommandContext ctx)
        {
            ushort x = 0;
            ushort y = 0;
            var z = ctx.Callee.Transform.Z;

            if (!ctx.Read(p =>
            {
                p.ReadNumber("x coordinate", ref x);
                p.ReadNumber("y coordinate", ref y);
                p.ReadNumber("z coordinate", ref z, true);
            }))
                return;

            ctx.Callee.ForceTeleport(x, y, z);
        }

        [CommandMethod]
        public void Logout(CommandContext ctx)
        {
            ctx.Callee.Logout(out _);
        }

        [CommandMethod("flogout")]
        public void ForcedLogout(CommandContext ctx)
        {
            ctx.Callee.ForcedLogout();
        }

        [CommandMethod]
        public void Id(CommandContext ctx)
        {
            foreach (var obs in ctx.Callee.Observatory)
                ctx.Callee.SendSystemChatMessage($"{obs}");
        }
    }
}