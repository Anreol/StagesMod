using RoR2;
using UnityEngine;

namespace StagesMod.ScriptableObjects
{
    //EffectDefs aren't scriptable Objects atm so this is a workaround
    [CreateAssetMenu(fileName = "EffectDef Holder", menuName = "StagesMod/EffectDef Holder", order = 2)]
    public class EffectDefHolder : ScriptableObject
    {
        public GameObject[] effectPrefabs;

        public static EffectDef ToEffectDef(GameObject effect)
        {
            return new EffectDef(effect);
        }
    }
}