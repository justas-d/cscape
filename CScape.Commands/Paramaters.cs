using System;

namespace CScape.Basic.Commands
{
    public static class Paramaters
    {
        public static bool Read(this CommandContext ctx, Action<ParamaterLexer> builder)
        {
            try
            {
                var lexer = new ParamaterLexer(ctx.Data);
                builder(lexer);

                if (lexer.DidFail)
                {
                    if (lexer.FailParamExpected != null)
                    {
                        ctx.Callee.SendSystemChatMessage(
                            $"Invalid type for argument {lexer.FailedOnParam}. Expected: {lexer.FailParamExpected.Name}.");
                    }
                    else
                        ctx.Callee.SendSystemChatMessage($"Missing argument: {lexer.FailedOnParam}.");

                    return false;
                }

                return true;
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