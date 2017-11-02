using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

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

        public int MaxVisibleEntities { get; set; } = 256;

        // 2 tile step in order to half the number of calls to GetVisibleEntities 
        // when the optimal viewrange we are searching for approaches zero and our 
        // current optimal viewrange approaches the maximum viewrange
        // e.g instead of 15 14 13 12 ... 1 we can do 15 13 11 9 .. 1
        private const int Step = 2;
        private const int HalfStep = 2;

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

        private int GetNumberOfVisibleEntitiesWithViewrange(int viewrange, IList<IEntityHandle> entities)
            => entities.Count(ent => EntityVision.IsEntityWithinViewrangeOfOther(Parent, ent.Get(), viewrange));

        // searches for a viewrange that we can plug into EntityVision.GetVisibleEntities and 
        // get a number of entities that is as close as possible to MaxVisibleEntities without 
        // ever going over it
        public IEnumerable<IEntityHandle> GetVisibleEntities()
        {
            var visibleEntities = GetEnumeratedVisibleEntities();

            if (visibleEntities.Count == MaxVisibleEntities)
                return visibleEntities;

            if (!TryDecreaseOptimalViewrange(visibleEntities))
            {
                var increase = TryIncreaseOptimalViewrange(visibleEntities);
                Debug.Assert(increase);
            }

            return EntityVision.GetVisibleEntities(Parent, OptimalViewrange);            
        }


        private bool IsUnder(int currentCount) => MaxVisibleEntities > currentCount;
        private bool IsOver(int currentCount) => currentCount > MaxVisibleEntities;

        private bool TryIncreaseOptimalViewrange([NotNull] IList<IEntityHandle> visibleEntities)
            => StepSearch(visibleEntities, IsUnder, IsOver, ovr => ovr >= MaxViewRange, Step, HalfStep);

        private bool TryDecreaseOptimalViewrange([NotNull] IList<IEntityHandle> visibleEntities)
            => StepSearch(visibleEntities, IsOver, IsUnder, ovr => 0 >= ovr, -Step, -HalfStep);

        private bool StepSearch(
            [NotNull] IList<IEntityHandle> visibleEntities,
            [NotNull] Func<int, bool> searchInvariant,
            [NotNull] Func<int, bool> checkIfCurrentCountIsNearOptimalButNotOptimal,
            [NotNull] Func<int, bool> isOptimalViewrangeAtTheEnd,
            int step,
            int halfStep)
        {
            if (visibleEntities == null) throw new ArgumentNullException(nameof(visibleEntities));
            if (searchInvariant == null) throw new ArgumentNullException(nameof(searchInvariant));
            if (checkIfCurrentCountIsNearOptimalButNotOptimal == null) throw new ArgumentNullException(nameof(checkIfCurrentCountIsNearOptimalButNotOptimal));
            if (isOptimalViewrangeAtTheEnd == null) throw new ArgumentNullException(nameof(isOptimalViewrangeAtTheEnd));
            if (visibleEntities == null) throw new ArgumentNullException(nameof(visibleEntities));

            var currentCount = visibleEntities.Count;
            var wasWorkDone = false;

            while (searchInvariant(currentCount))
            {
                wasWorkDone = true;

                OptimalViewrange += step;
                currentCount = GetNumberOfVisibleEntitiesWithViewrange(OptimalViewrange, visibleEntities);

                if (checkIfCurrentCountIsNearOptimalButNotOptimal(currentCount))
                {
                    // the optimal viewrange is in [OptimalViewrange - Step; OptimalViewrange)

                    // check if OptimalViewrange -= HalfStep is optimal
                    OptimalViewrange -= halfStep;

                    if (checkIfCurrentCountIsNearOptimalButNotOptimal(
                        GetNumberOfVisibleEntitiesWithViewrange(OptimalViewrange, visibleEntities)))
                        OptimalViewrange -= halfStep; // it's not, therefore OptimalViewrange - Step is optimal

                    return true;
                }

                // if we're going out of bounds, bail
                if (isOptimalViewrangeAtTheEnd(OptimalViewrange))
                    return true;
            }

            Debug.Assert(wasWorkDone == false);
            return false;
        }

        [NotNull]
        private IList<IEntityHandle> GetEnumeratedVisibleEntities()
        {
            var unenumeratedVisibleEntities = EntityVision.GetVisibleEntities(Parent, OptimalViewrange);
            var visibleEntities = unenumeratedVisibleEntities as IList<IEntityHandle> ?? unenumeratedVisibleEntities.ToList();
            Debug.Assert(visibleEntities != null);
            return visibleEntities;
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            
        }
    }
}