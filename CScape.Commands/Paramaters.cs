using System;
using CScape.Models.Game.Entity;

namespace CScape.Commands
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
                        ctx.Callee.SystemMessage(
                            $"Invalid type for argument {lexer.FailedOnParam}. Expected: {lexer.FailParamExpected.Name}.", SystemMessageFlags.Normal | CommandSystemMessageType.Id);
                    }
                    else
                        ctx.Callee.SystemMessage($"Missing argument: {lexer.FailedOnParam}.", SystemMessageFlags.Normal | CommandSystemMessageType.Id);

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ctx.Callee.SystemMessage("Command paramater error.", SystemMessageFlags.Normal | CommandSystemMessageType.Id);
                ctx.Callee.SystemMessage($"Paramaters.Read exception: {ex}", SystemMessageFlags.Debug | CommandSystemMessageType.Id);
            }
            return false;
        }
    }
}