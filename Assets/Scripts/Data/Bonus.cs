using System;

namespace SB.Data
{
    [Serializable]
    public class Bonus
    {
        [Serializable]
        public class Levelup
        {
            public string description;
        }

        [Serializable]
        public class BonusInfo : PowerUp
        {            
            public float baseHpChange;
            public float baseScaleChange;
            public float baseDurationChange;
            public int baseAttackChange;
            public float baseAttackSpeedChange;
            public int baseProjectileChange;
            public float baseCooldownChange;
            public float baseExperienceChange;
            public int baseDefenseChange;
            public float baseHPSChange;
            public float baseProjectileSpeedChange;
            public float baseMoveSpeedChange;

            public Levelup[] levelup;

            public void RefreshDescription()
            {
                description = (levelup.Length > 0 && level > 1) ? levelup[level - 2].description : description;
            }

            public bool IsLevelMaxed()
            {
                return (level >= levelup.Length + 1);
            }

            public Levelup GetCurrentLevel()
            {
                return (levelup[level - 1]);
            }
        }

        public BonusInfo[] bonus;
    }
}
