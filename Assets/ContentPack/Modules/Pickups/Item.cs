using RoR2;

namespace StagesMod.Items
{
    public abstract class Item
    {
        public abstract ItemDef itemDef { get; set; }

        /// <summary>
        /// For the love of god PLEASE use this as minimally as possible for hooks, use itemBehaviors wherever possible
        /// </summary>
        public virtual void Initialize()
        {
        }

        public virtual void AddBehavior(ref CharacterBody body, int stack)
        {
        }
    }
}