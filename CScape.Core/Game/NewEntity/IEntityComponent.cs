﻿using CScape.Core.Data;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    /// <summary>
    /// Updates run during the entity update pass
    /// </summary>
    public interface IEntityComponent : IEntityFragment
    {
        void Update([NotNull]IMainLoop loop);
    }
}