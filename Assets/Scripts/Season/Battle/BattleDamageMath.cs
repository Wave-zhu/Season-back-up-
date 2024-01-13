using UnityEngine;

namespace Season.Battle
{
    public static class BattleDamageMath
    {
        public static float BreakGaugeFactor(float percent)
        {
            return percent switch
            {
                >= 1f => 0.2f,
                >= 0.6f => 1f - (-5 * percent * percent + 10 * percent - 4.2f),
                >= 0.2f => 1f - (-6.25f * percent * percent + 8.75f * percent - 3f),
                _ => 2.5f
            };
        }

        public static float SkillAttackFactor(int skillDamage)
        {
            float baseMultiplier = Mathf.Log(10) / 400f;
            float multiplier = Mathf.Exp(baseMultiplier * (skillDamage - 100f));
            return multiplier;
        }

        //todo about random, lucky system(maybe if i have time)
        public static float BaseDamage(float attack, float defense)
        {
            return attack * (1 - defense / (100f + defense)) * Random.Range(0.95f, 1.1f);
        }

        public static float BaseRestore(float attack, float defense)
        {
            return defense * (1 + attack / (100f + attack)) * Random.Range(0.95f, 1.1f);
        }

        public static int RecoverAccumulate(float breakGaugeFactor, float skillAttackFactor, float multi)
        {
            return Mathf.RoundToInt(breakGaugeFactor * 10 * skillAttackFactor * multi);
        }
        
        
        
    }
}