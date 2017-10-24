using System;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Interface;
using JetBrains.Annotations;

namespace CScape.Core.Extensions
{
    public static class InterfaceExtensions
    {
        public static void ShowSidebar(
            [NotNull] this IInterfaceComponent interfaces,
            [NotNull] IGameInterface interf,
            byte sidebarIdx)
        {
            if (interfaces == null) throw new ArgumentNullException(nameof(interfaces));
            if (interf == null) throw new ArgumentNullException(nameof(interf));

            interfaces.Show(InterfaceMetadata.Sidebar(interf, sidebarIdx));
        }
    }
}
