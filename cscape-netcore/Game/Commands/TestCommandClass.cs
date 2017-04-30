namespace CScape.Game.Commands
{
    [CommandsClass]
    public sealed class TestCommandClass
    {
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
            ctx.Callee.Movement.IsRunning = true;
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
            player.SendSystemChatMessage($"X: {player.Position.X} Y: {player.Position.Y} Z: {player.Position.Z}");
            player.SendSystemChatMessage($"LX: {player.Position.LocalX} LY: {player.Position.LocalY}");
            player.SendSystemChatMessage($"RX: {player.Position.RegionX} + 6 RY: {player.Position.RegionY} + 6");
        }

        [CommandMethod("pos set")]
        public void SetPos(CommandContext ctx)
        {
            // todo : params for commands
        }

        [CommandMethod]
        public void Logout(CommandContext ctx)
        {
            ctx.Callee.Logout(out _);
        }

        [CommandMethod("logout force")]
        public void ForcedLogout(CommandContext ctx)
        {
            ctx.Callee.ForcedLogout();
        }

        [CommandMethod]
        public void Id(CommandContext ctx)
        {

            ctx.Callee.SendSystemChatMessage($"UEI: {ctx.Callee.UniqueEntityId}");
            ctx.Callee.SendSystemChatMessage($"PID: {ctx.Callee.Pid}");
        }
    }
}