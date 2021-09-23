using RoR2;

namespace StagesMod
{
    public interface IStatBuffBehavior
    {
        void RecalculateStatsEnd();

        void RecalculateStatsStart(ref CharacterBody characterBody);
    }
}