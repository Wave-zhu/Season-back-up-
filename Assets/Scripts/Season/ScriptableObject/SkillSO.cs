using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace Season.ScriptableObject
{
    public enum AttackRange
    {
        ALL,
        FAN_L,
        FAN_SL,
        CIRCLE_S,
        CIRCLE_M,
        CIRCLE_L,
        LINE_M,
        LINE_L,
        SINGLE,
    }

    public enum EffectType
    {
        SELF_ONLY,
        ALL,
        ALL_IGNORE_SELF,
        PLAYER_ALL,
        PLAYER_IGNORE_SELF,
        ENEMY_ALL,
        ENEMY_IGNORE_SELF
    }

    public enum PowerLevel
    {
        NORMAL,
        HIGH,
        VERY_HIGH,
        DESTRUCTIVE,
    }
    public enum ResultType
    {
        NORMAL, //cut bd then hp
        MAGIC, //only cut hp 
        BLUNT, //only cut bd
        BREAK_RESTORE,
        HEAL,
        SUPER_BREAK_RESTORE,
        SUPER_HEAL
    }
    [Flags]
    public enum Debuff
    {
        NONE = 0,
        POISON = 1 << 0,
        FROZEN = 1 << 1, // cannot move
        PARALYZE =1 << 2, // broadcast when pass to paralyze
        BURN = 1 << 3,
    }
    [Flags]
    public enum Buff
    {
        NONE = 0,
        WEAK = 1 << 0, // damage down
        VULNERABLE = 1 << 1,// damage received up
        DIZZY = 1 << 2,
        BLOW = 1 << 3,
        DISPLACEMENT = 1 << 4,
    }

    [Flags]
    public enum InheritEffectType
    {
        NONE = 0,
        PRIORITY = 1 << 0,
        RANGE = 1 << 1,
        STATUS_CONDITION = 1 << 2,
        TYPE = 1 << 3,
    }
    [CreateAssetMenu(fileName = "SkillData", menuName = "Create/SkillData", order = 0)]
    public class SkillSO : UnityEngine.ScriptableObject
    {
        public string skillName;
    
        public AttackRange attackRange;
    
        public EffectType effectType;

        public PowerLevel powerLevel;

        public bool IsHighPower => powerLevel == PowerLevel.VERY_HIGH || powerLevel == PowerLevel.DESTRUCTIVE;
    
        public ResultType resultType;
    
        public Debuff debuffType;

        public Buff buffType;

        public InheritEffectType inheritEffectType;
    
        //TODO skill displacement
        public bool hasDisplacementOnCast;
    
        public float maxCastRange;
    
        public int baseHealthValue;

        public int baseBreakGaugeValue;

        public int baseAccumulateCost;

        public float accumulateGainFactor = 1f;
    
        public string hitName;
    
        public List<CharacterNameEnum> friendPassList;
    
        public int priorityInfo;

        public TimelineAsset cutscene;

        public float minOffset = -1f;

        public float maxOffset = 2f;
    }
}