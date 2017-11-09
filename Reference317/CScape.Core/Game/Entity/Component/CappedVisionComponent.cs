using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CScape.Core.Extensions;
using CScape.Core.Utility;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using JetBrains.Annotations;
using CoreSystemMessageFlags = CScape.Core.Game.Entity.Message.CoreSystemMessageFlags;

namespace CScape.Core.Game.Entity.Component
{
    public sealed class CappedVisionComponent : EntityComponent, IVisionComponent
    {
        private int _optimalViewrange;
        public override int Priority => (int) ComponentPriority.CappedVision;
        
        int IVisionComponent.ViewRange
        {
            get => MaxViewRange;
            set => MaxViewRange = value;
        }
        
        public int MaxViewRange { get; set; } = EntityVision.DefaultViewRange;

        private int OptimalViewrange
        {
            get => _optimalViewrange;
            set => _optimalViewrange = value.Clamp(0, MaxViewRange);
        }

        public int MaxVisibleEntities { get; set; } = 1;

        public CappedVisionComponent([NotNull] IEntity parent) : base(parent)
        {
            OptimalViewrange = MaxViewRange;
        }

        public bool CanSee(IEntity ent)
        {
            // recalc optimal viewrange
            GetVisibleEntities();
                
            return EntityVision.CanSee(Parent, ent, OptimalViewrange);
        }

        // searches for a viewrange that we can plug into EntityVision.GetVisibleEntities and 
        // get a number of entities that is as close as possible to MaxVisibleEntities without 
        // ever going over it
        public IEnumerable<IEntityHandle> GetVisibleEntities()
        {
            var oldOptimal = OptimalViewrange;

            OptimalViewrange = FindMostOptimalViewrange();

            if (oldOptimal != OptimalViewrange)
                Parent.SystemMessage($"Optimal vr: {oldOptimal} -> {OptimalViewrange}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Entity);

            return EntityVision.GetVisibleEntities(Parent, OptimalViewrange);            
        }
        
        private int FindMostOptimalViewrange()
        {
            var maxVisibleEntities = GetEnumeratedMaxVisibleEntities();

            if (maxVisibleEntities.Count == MaxVisibleEntities)
                return MaxViewRange;
            
            int ValueRetriever(int viewrange)
                => maxVisibleEntities.Count(ent => EntityVision.CanSee(Parent, ent.Get(), viewrange));
            
            var optimalResults = Algorithm.ProgressiveBinarySearch(
                ValueRetriever,
                MaxVisibleEntities,
                0, 
                MaxViewRange);

            var best = FindBestSolution(optimalResults);
            return best.viewrange;
        }

        private IList<IEntityHandle> GetEnumeratedMaxVisibleEntities()
        {
            var unenumeratedMaxVisibleEntities = EntityVision.GetVisibleEntities(Parent, MaxViewRange);
            var maxVisibleEntities = unenumeratedMaxVisibleEntities as IList<IEntityHandle> ??
                                     unenumeratedMaxVisibleEntities.ToList();

            Debug.Assert(maxVisibleEntities != null);

            return maxVisibleEntities;
        }

        private (int viewrange, int entityCount) FindBestSolution(IEnumerable<(int index, int value)> optimalResults)
        {
            (int viewrange, int entityCount) best = (0,0);

            foreach ((int viewrange, int entityCount) d in optimalResults)
            {
                if (MaxVisibleEntities >= d.entityCount)
                {
                    if (d.entityCount > best.entityCount)
                        best = d;
                }
            }

            return best;
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            // ignored
        }
    }
}