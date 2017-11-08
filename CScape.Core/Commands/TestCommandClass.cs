using System;
using System.Collections.Generic;
using CScape.Commands;
using CScape.Core.Extensions;
using CScape.Core.Game;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Game.Skill;
using CScape.Core.Game.World;
using CScape.Core.Json;
using CScape.Core.Network.Entity.Component;
using CScape.Models;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity.FacingData;
using CScape.Models.Game.Entity.Factory;
using CScape.Models.Game.Item;
using CScape.Models.Game.World;

namespace CScape.Core.Commands
{


    [CommandsClass]
    public sealed class TestCommandClass
    {
        [CommandMethod("maxent")]
        public void SetMaxEntities(CommandContext ctx)
        {
            int maxEnts = 0;
            if (!ctx.Read(b =>
            {
                b.ReadNumber("max ents", ref maxEnts);
            })) return;

            var vision = ctx.Callee.Parent.Components.Get<CappedVisionComponent>();
            if (vision == null)
            {
                ctx.Callee.Parent.SystemMessage("No vision component found.");
                return;
            }


            vision.MaxVisibleEntities = maxEnts;
            ctx.Callee.Parent.SystemMessage($"Max ents: {vision.MaxVisibleEntities}");
        }


        [CommandMethod("dir")]
        public void SetDir(CommandContext ctx)
        {
            byte rawDir = 0;
            if (!ctx.Read(b =>
            {
                b.ReadNumber("direction", ref rawDir);
            })) return;

            var dir = (Direction) rawDir;
            var t = ctx.Callee.Parent.GetTransform();
            t.SetFacingDirection(new DirecionFacingState(new DirectionDelta(dir), t));
        }

        [CommandMethod("debug")]
        public void ToggleDebug(CommandContext ctx)
        {
            var cmp = ctx.Callee.Parent.Components;
            if (cmp.Contains<DebugStatNetworkSyncComponent>())
                cmp.Remove<DebugStatNetworkSyncComponent>();
            else
                cmp.Add(new DebugStatNetworkSyncComponent(ctx.Callee.Parent));
        }

        [CommandMethod("gain")]
        public void GainExp(CommandContext ctx)
        {
            var amount = 0;
            if (!ctx.Read(b =>
            {
                b.ReadNumber("exp", ref amount);
            })) return;

            var skills = ctx.Callee.Parent.AssertGetSkills();
            var db = ctx.Callee.Parent.Server.Services.ThrowOrGet<SkillDb>();
            skills.GainExperience(db.Agility, amount);
        }

        [CommandMethod("ftext")]
        public void ForcedText(CommandContext ctx)
        {
            if (string.IsNullOrEmpty(ctx.Data)) return;

            ctx.Callee.ForceChatMessage(ChatMessage.ForceSay(ctx.Data, ctx.Callee));
        }

        [CommandMethod("dmg")]
        public void TestDmgF(CommandContext ctx)
        {
            short pid = 0;
            byte dmg = 0;
            byte type = 0;

            if (!ctx.Read(b =>
            {
                b.ReadNumber("pid", ref pid);
                b.ReadNumber("damage", ref dmg);
                b.ReadNumber("type", ref type);
            })) return;

            var players = ctx.Callee.Parent.Server.Services.ThrowOrGet<IPlayerCatalogue>();
            var ent = players.Get(pid);
            if (ent == null)
            {
                ctx.Callee.Parent.SystemMessage("Player not found.");
                return;
            }
            ent.Get().AssertGetHealth().TakeDamage(dmg, type);
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


         
            var npcs = ctx.Callee.Parent.Server.Services.ThrowOrGet<INpcFactory>();
            var ent= npcs.Get(uni);
            if (ent == null)
            {
                ctx.Callee.Parent.SystemMessage("Npc not found.");
                return;
            }

            ent.Get().ShowParticleEffect(new ParticleEffect(effId, effHeight, effDelay));

        }

        [CommandMethod("anim")]
        public void SetPlayerAnim(CommandContext ctx)
        {
            short id = 0;
            short animId = 0;
            byte delay = 0;

            if (!ctx.Read(b =>
            {
                b.ReadNumber("player id", ref id);
                b.ReadNumber("animation id", ref animId);
                b.ReadNumber("delay", ref delay);
            })) return;

            var players = ctx.Callee.Parent.Server.Services.ThrowOrGet<IPlayerCatalogue>();
            var ent = players.Get(id);
            if (ent == null)
            {
                ctx.Callee.Parent.SystemMessage("Player not found.");
                return;
            }

            ent.Get().ShowAnimation(new Animation(animId, delay));
        }

        [CommandMethod("npc")]
        public void SpawnNpc(CommandContext ctx)
        {
            short defId = 0;
            if (!ctx.Read(b => b.ReadNumber("definition id", ref defId))) return;

            var factory = ctx.Callee.Parent.Server.Services.ThrowOrGet<INpcFactory>();
            var npc = factory.Create("Spanwed NPC", defId);

            npc.Get().GetTransform().Teleport(ctx.Callee.Parent.GetTransform());
            
            ctx.Callee.Parent.SystemMessage($"Spawned NPC. Intance ID: {npc.Get().AssertGetNpc().InstanceId}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Entity);
        }

        [CommandMethod("clearinv")]
        public void ClearInv(CommandContext ctx)
        {
            var inv = ctx.Callee.Parent.AssertGetInventory();

            for (var i = 0; i < inv.Inventory.Provider.Count; i++)
                inv.Inventory.ExecuteChangeInfo(ItemChangeInfo.Remove(i));
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

            var inv = ctx.Callee.Parent.AssertGetInventory();
            var db = ctx.Callee.Parent.Server.Services.ThrowOrGet<ItemDatabase>();

            for (var i = 0; i < count; i++)
            {
                int id;
                do
                {
                    id = rng.Next(0, max);
                } while (used.Contains(id));
                used.Add(id);

                inv.Inventory.ExecuteChangeInfo(
                    inv.Inventory.CalcChangeInfo(new ItemStack(db.Get(id), rng.Next(int.MaxValue))));
            }
        }

        [CommandMethod("close")]
        public void CloseServer(CommandContext ctx)
        {
            ctx.Callee.Parent.Server.Dispose();
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

            var inv = ctx.Callee.Parent.AssertGetInventory();
            var db = ctx.Callee.Parent.Server.Services.ThrowOrGet<ItemDatabase>();

            var change = inv.Inventory.CalcChangeInfo(new ItemStack(db.Get(id), amount));
            inv.Inventory.ExecuteChangeInfo(change);

            ctx.Callee.Parent.SystemMessage($"Giving {amount} with overflow {change.OverflowAmount}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Item);
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

            var inv = ctx.Callee.Parent.AssertGetInventory();
            var db = ctx.Callee.Parent.Server.Services.ThrowOrGet<ItemDatabase>();

            inv.Inventory.ExecuteChangeInfo(new ItemChangeInfo(idx, new ItemStack(db.Get(id), amount), 0));
        }

        [CommandMethod("poe now")]
        public void PrintPoe(CommandContext ctx)
        {
            ctx.Callee.Parent.SystemMessage($"PoE: {ctx.Callee.Parent.GetTransform().PoE.Name}");
        }

        private PlaneOfExistence _diffPoe;

        [CommandMethod("poe test")]
        public void SwitchPoe(CommandContext ctx)
        {
            if (_diffPoe == null)
                _diffPoe = new PlaneOfExistence(ctx.Callee.Parent.Server, "test_poe");

            ctx.Callee.Parent.GetTransform().SwitchPoE(_diffPoe);
        }

        [CommandMethod("ow")]
        public void PoeOverworld(CommandContext ctx)
        {
            var t = ctx.Callee.Parent.GetTransform();
            t.SwitchPoE(ctx.Callee.Parent.Server.Overworld);
        }

        [CommandMethod("tickrate")]
        public void SetTickRate(CommandContext ctx)
        {
            var tickrate = 0;
            if (!ctx.Read(p => p.ReadNumber("tickrate", ref tickrate))) return;
            ctx.Callee.Parent.Server.Services.ThrowOrGet<IMainLoop>().TickRate = tickrate;
        }

        [CommandMethod]
        public void Run(CommandContext ctx)
        {
            var movement = ctx.Callee.Parent.Components.AssertGet<TileMovementComponent>();
            movement.IsRunning = !movement.IsRunning;
        }

        [CommandMethod("pos get")]
        public void GetPos(CommandContext ctx)
        {
            var t = ctx.Callee.Parent.GetTransform();
            var client = ctx.Callee.Parent.AssertGetClientPosition();
            var ent = ctx.Callee.Parent;

            ent.SystemMessage($"X: {t.X} Y: {t.Y} Z: {t.Z}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Entity);
            ent.SystemMessage($"REGX: {t.Region.X} REGY: {t.Region.Y}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Entity);
            ent.SystemMessage($"LX: {client.Local.X} LY: {client.Local.Y}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Entity);
            ent.SystemMessage($"CRX: {client.ClientRegion.Y} + 6 CRY: {client.ClientRegion.Y} + 6", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Entity);
            
        }

        [CommandMethod("pos set")]
        public void SetPos(CommandContext ctx)
        {
            var t = ctx.Callee.Parent.GetTransform();

            ushort x = 0;
            ushort y = 0;
            var z = t.Z;

            if (!ctx.Read(p =>
            {
                p.ReadNumber("x coordinate", ref x);
                p.ReadNumber("y coordinate", ref y);
                p.ReadNumber("z coordinate", ref z, true);
            }))
                return;

            t.Teleport(x,y,z);
        }

        [CommandMethod]
        public void Logout(CommandContext ctx)
        {
            ctx.Callee.TryLogout();
        }

        [CommandMethod("flogout")]
        public void ForcedLogout(CommandContext ctx)
        {
            ctx.Callee.ForcedLogout();
        }
    }
}