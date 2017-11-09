using System.Collections.Generic;
using System.Reflection;
using CScape.Models.Game.Command;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Commands.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void WithCommandHandler(this IServiceCollection builder, [CanBeNull] IEnumerable<Assembly> assemblyContainingCommands)
        {
            builder.AddSingleton<ICommandHandler>(s =>
            {
                var a = new CommandDispatch();
                if (assemblyContainingCommands != null)
                {
                    foreach (var asm in assemblyContainingCommands)
                    {
                        a.RegisterAssembly(asm);
                    }
                }
                
                return a;
            });

        }
    }
}
