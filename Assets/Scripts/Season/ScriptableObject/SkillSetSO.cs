using System.Collections.Generic;
using UnityEngine;

namespace Season.ScriptableObject
{
    [CreateAssetMenu(fileName = "SkillSetData", menuName = "Create/SkillSetData", order = 0)]
    public class SkillSetSO : UnityEngine.ScriptableObject
    {
        public List<SkillSO> skillSet = new List<SkillSO>();
    }
}
