using System;

namespace CScape.Game.Commands
{
    public static class Paramaters
    {
        public static bool Read(CommandContext ctx, Action<ParamaterLexer> builder)
        {
            try
            {
                builder(new ParamaterLexer(ctx.Data));
                return true;
            }
            catch (ParamaterLexer.ParamParseException pex)
            {
                ctx.Callee.SendSystemChatMessage(
                    $"Invalid type for argument {pex.ParamName}. Expected: {pex.ParamType}.");
            }
            catch (ParamaterLexer.ParamNotFoundException nfex)
            {
                ctx.Callee.SendSystemChatMessage($"Missing argument: {nfex.ParamName}.");
            }
            catch (Exception ex)
            {
                if (ctx.Callee.DebugCommands)
                    ctx.Callee.SendSystemChatMessage($"Paramaters.Read exception: {ex}");
                else
                    ctx.Callee.SendSystemChatMessage($"Command paramater error.");
            }
            return false;
        }
    }
}