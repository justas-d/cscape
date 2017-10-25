using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CScape.Models.Extensions;
using CScape.Models.Game.Command;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Commands
{
    public sealed class CommandDispatch  : ICommandHandler
    {
        private readonly Dictionary<string, Command> _cmds = new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase);

        public CommandDispatch()
        {
            RegisterAssembly(GetType().GetTypeInfo().Assembly);
        }

        public void RegisterAssembly(Assembly asm)
        {
            foreach (var cls in asm.GetTypes())
            {
                var clsInfo = cls.GetTypeInfo();
                if (clsInfo.GetCustomAttribute<CommandsClassAttribute>() == null)
                    continue;

                var clsPredicates = clsInfo.GetCustomAttributes<PredicateAttribute>().ToList();

                foreach (var method in cls.GetRuntimeMethods())
                {
                    var cmdAttrib = method.GetCustomAttribute<CommandMethodAttribute>();
                    if (cmdAttrib == null)
                        continue;

                    var id = cmdAttrib.Identifier ?? method.Name.ToLowerInvariant();
                    var instance = Activator.CreateInstance(cls);

                    // verify signature of method
                    var args = method.GetParameters();
                    Action noArgExec = null;
                    Action<CommandContext> exec = null;

                    if (args.Length == 1)
                    {
                        if (args[0].ParameterType != typeof(CommandContext))
                            throw new InvalidOperationException(
                                "Command method can either have no args or must take a CommandContext.");

                        exec = (Action<CommandContext>)method.CreateDelegate(typeof(Action<CommandContext>), instance);
                    }
                    else if (args.Length != 0)
                        throw new InvalidOperationException(
                            "Command method can either have no args or must take a CommandContext.");
                    else
                        noArgExec = (Action)method.CreateDelegate(typeof(Action), instance);

                    RegisterCommand(new Command(id, noArgExec, exec, clsPredicates.Concat(method.GetCustomAttributes<PredicateAttribute>())));
                }
            }
        }

        public void RegisterCommand([NotNull] Command command)
        {
            if (_cmds.ContainsKey(command.Identifier))
                throw new InvalidOperationException($"Duplicate command with identifier {command.Identifier}");

            _cmds.Add(command.Identifier, command);
        }

        [CanBeNull]
        public Command GetCommand(string id)
        {
            if (!_cmds.ContainsKey(id)) return null;
            return _cmds[id];
        }

        public bool Push(IEntity callee, string input)
        {
            if (callee == null) throw new ArgumentNullException(nameof(callee));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var player = callee.GetPlayer();

            if (player == null)
                return false;

            try
            {
                // find command in input
                var identifier = "";
                foreach (var word in input.Split(' ').Select(s => s.Trim()))
                {
                    if (string.IsNullOrEmpty(word))
                        continue;

                    identifier += word;

                    var cmd = GetCommand(identifier);
                    if (cmd == null)
                    {
                        identifier += " ";
                        continue;
                    }
                    // cmd found

                    // check if predicates say its ok to proceed.
                    if (cmd.Predicates.Any(pred => !pred.CanExecute(callee, cmd)))
                        break;

                    // parse data if needed
                    string data = null;
                    if (cmd.NoArgExecTarg == null)
                        data = input.Substring(input.IndexOf(word, StringComparison.Ordinal) + word.Length).TrimStart();

                    try
                    {
                        // dispatch cmd
                        if (cmd.NoArgExecTarg != null)
                            cmd.NoArgExecTarg();
                        else if (cmd.ExecTarg != null)
                            cmd.ExecTarg(new CommandContext(player, cmd, data, input));
                        else
                            throw new NotSupportedException("Cmd has no exec target.");
                    }
                    catch (Exception)
                    {
                        callee.SystemMessage("Command error. Make sure inputs are valid.", SystemMessageFlags.Normal | CommandSystemMessageType.Id);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                callee.SystemMessage($"cmd fail: callee: data: {input} ex: {ex}",
                    SystemMessageFlags.Debug | CommandSystemMessageType.Id);

                callee.SystemMessage("Command parse error.", SystemMessageFlags.Normal | CommandSystemMessageType.Id);

                return true;
            }

            return false;
        }
    }
}