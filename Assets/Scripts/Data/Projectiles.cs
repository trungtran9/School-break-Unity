using System;

namespace SB.Data
{
    [Serializable]
    public class Projectiles
    {
        [Serializable]
        public class Levelup
        {
            public int level;
            public string description;
            public int dmgChange;
            public float dmgChangePct;
            public float radiusChange;
            public float sizeChange;
            public float speedChange;
            public int projectileAmount;
            public float cooldownChange;
            public float scaleChange;
            public float durationChange;
        }

        [Serializable]
        public class Bullet : PowerUp
        {
            public string explosion;
            public int minBaseDamage;
            public int maxBaseDamage;            
            public float projectileSpeed;
            public float scale = 0f;
            public string bulletType;                      
            public string target;
            public float impactCooldown;
            public int baseAmount;
            public float duration;
            public int ignoreDuration = 0;
            public float baseRadius;
            public float knockbackForce;
            public float cooldown;            
            public int basePenetrationTimes;
            public float tangentForce;
            public float delay;
            public float friction;
            public int spread;
            public string buffApply;
            public string shootSound;
            public string explosionSound;
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

        public Bullet[] bullet;
    }
}