using System;
using System.Collections.Generic;
using CScape.Basic.Commands;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Game.World;
using CScape.Core.Network.Packet;

namespace CScape.Core.Commands
{
    [CommandsClass]
    public sealed class TestCommandClass
    {
        private PlaneOfExistence _diffPoe;

        [CommandMethod("sight get")]
        public void GetSight(CommandContext ctx)
        {
            ctx.Callee.SendSystemChatMessage($"View range: {ctx.Callee.ViewRange}");
        }

        [CommandMethod("sight set")]
        public void SetSight(CommandContext ctx)
        {
            var sight = 0;

            if (!ctx.Read(b =>
            {
                b.ReadNumber("sight", ref sight);
            })) return;

            ctx.Callee.ViewRange= sight;
        }

        [CommandMethod("gain")]
        public void GainExp(CommandContext ctx)
        {
            var amount = 0;
            if (!ctx.Read(b =>
            {
                b.ReadNumber("exp", ref amount);
            })) return;

            ctx.Callee.Skills.Agility.GainExperience(amount);
        }

        [CommandMethod("dropclear")]
        public void TestGroundClear(CommandContext ctx)
        {
            ctx.Callee.Connection.SendPacket(
                new ResetGroundObjectsInRegionPacket(
                    ctx.Callee.ClientTransform.Local));
        }

        [CommandMethod("dropspam")]
        public void TestDropPacket(CommandContext ctx)
        {
            IEnumerable<BaseGroundObjectPacket> Random()
            {
                var rng = new Random();
                const int max = BaseGroundObjectPacket.MaxOffset;
                for (byte x = 0; x <= max; x++)
                {
                    for (byte y = 0; y <= max; y++)
                    {
                        var packet = new SpawnGroundItemPacket(
                            (rng.Next(1, 4000), rng.Next(1, 4000)),
                            x, y);
                        yield return packet;
                    }
                }
            }

            var wrapper = new EmbeddedRegionGroundObjectWrapperPacket(
                ctx.Callee.ClientTransform.Local, Random());

            ctx.Callee.Connection.SendPacket(wrapper);
        }

        [CommandMethod("drop")]
        public void TestItemDrop(CommandContext ctx)
        {
            short id = 0;
            short amnt = 0;
            byte x = 0;
            byte y = 0;

            if (!ctx.Read(b =>
            {
                b.ReadNumber("id", ref id);
                b.ReadNumber("amount", ref amnt);
                b.ReadNumber("x", ref x);
                b.ReadNumber("y", ref y);
            })) return;

            var packet = new SpawnGroundItemPacket((id, amnt), x, y);
            ctx.Callee.SendSystemChatMessage($"IsInvalid: {packet.IsInvalid}");

            var wrapper = new EmbeddedRegionGroundObjectWrapperPacket(
                ctx.Callee.ClientTransform.Local, packet);

            ctx.Callee.Connection.SendPacket(wrapper);
        }

        [CommandMethod("ftext")]
        public void ForcedText(CommandContext ctx)
        {
            ctx.Callee.ForcedText = ctx.Data;
        }

        [CommandMethod("dmg")]
        public void TestNpcDmgFlag(CommandContext ctx)
        {
            short pid = 0;
            byte dmg = 0;
            byte type = 0;
            byte maxHealth = 0;
            var isSecondary = false;

            if (!ctx.Read(b =>
            {
                b.ReadNumber("pid", ref pid);
                b.ReadNumber("damage", ref dmg);
                b.ReadNumber("type", ref type);
                b.ReadBoolean("secondary ", ref isSecondary);
            })) return;

            var player = ctx.Callee.Server.Players.GetById(pid);
            if (player == null)
            {
                ctx.Callee.SendSystemChatMessage("Player not found.");
                return;
            }

            player.Damage(dmg, (HitType)type, isSecondary);
        }

        [CommandMethod("npceffect")]
        public void SetNpcParticleEffect(CommandContext ctx)
        {
            var uni = 0;
            short effId = 0;
            short effHeight = 0;
            short effDelay = 0;

            if (!ctx.Read(b =>
            {
                b.ReadNumber("UniqueNpcId", ref uni);
                b.ReadNumber("effect id ", ref effId);
                b.ReadNumber("effect height", ref effHeight);
                b.ReadNumber("effect delay", ref effDelay);
            })) return;

            var npc = ctx.Callee.Server.Npcs.GetById(uni);
            if (npc == null)
            {
                ctx.Callee.SendSystemChatMessage("Npc not found.");
                return;
            }

            npc.Effect = new ParticleEffect(effId, effHeight, effDelay);
        }

        [CommandMethod("anim")]
        public void SetPlayerAnim(CommandContext ctx)
        {
            short id = 0;
            short animId = 0;
            byte delay = 0;

            if (!ctx.Read(b =>
            {
                b.ReadNumber("ent id", ref id);
                b.ReadNumber("animation id", ref animId);
                b.ReadNumber("delay", ref delay);
            })) return;

            var ent = ctx.Callee.Server.Players.GetById(id);
            if (ent == null)
            {
                ctx.Callee.SendSystemChatMessage("ent not found.");
                return;
            }

            ent.Animation= new Animation(animId, delay);
        }

        [CommandMethod("npc")]
        public void SpawnNpc(CommandContext ctx)
        {
            short defId = 0;
            if (!ctx.Read(b => b.ReadNumber("definition id", ref defId))) return;

            var npc = new Npc(ctx.Callee.Server.Services, defId, ctx.Callee.Transform);
         //   npc.Movement.Directions = new FollowDirectionProvider(npc, ctx.Callee);
        }

        [CommandMethod("clearinv")]
        public void ClearInv(CommandContext ctx)
        {
            for (var i = 0; i < ctx.Callee.Inventory.Size; i++)
                ctx.Callee.Inventory.ExecuteChangeInfo(ItemChangeInfo.Remove(i));
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
                _diffPoe = new PlaneOfExistence(ctx.Callee.Server, "test_poe");

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