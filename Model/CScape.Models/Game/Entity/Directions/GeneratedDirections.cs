using CScape.Models.Game.World;

namespace CScape.Models.Game.Entity.Directions
{
    public struct GeneratedDirections
    {
        public DirectionDelta Walk { get; }
        public DirectionDelta Run { get; }

        public static GeneratedDirections Noop { get; } =
            new GeneratedDirections(DirectionDelta.Noop, DirectionDelta.Noop);

        public GeneratedDirections(DirectionDelta walk, DirectionDelta run)
        {
            // if walk is noop but run isin't, swap places
            if (walk.IsNoop() && !run.IsNoop())
            {
                Walk = run;
                Run = walk;
            }
            else
            {
                Walk = walk;
                Run = run;
            }
        }

        public bool IsNoop() => Walk.IsNoop() && Run.IsNoop();
    }
}