using System.Collections.Generic;
using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    /// <summary>
    /// Exposes the api backend for api interfaces
    /// </summary>
    public interface IInterfaceManagerApiBackend
    {
        [CanBeNull] IShowableInterface Main { get; set; }
        [CanBeNull] IShowableInterface Chat { get; set; }
        [CanBeNull] IShowableInterface Input { get; set; }

        [NotNull] IReadOnlyList<IShowableInterface> PublicSidebar { get; }
        [NotNull] IList<IShowableInterface> Sidebar { get; }

        [NotNull] IReadOnlyDictionary<int, IBaseInterface> PublicAll { get; }
        [NotNull] IDictionary<int, IBaseInterface> All { get; }

        [NotNull] IInterfaceManager Frontend { get; }

        void NotifyOfClose(IApiInterface interf);
    }
}