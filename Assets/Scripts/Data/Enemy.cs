using System;


namespace SB.Data
{
    /// <summary>
    /// Define the loot of fallen enmy with a percentage on it
    /// </summary>
    [Serializable]
    public class Loot
    {
        public string name;
        public int amount;
        public int percent;
    }

    [Serializable]
    public class Ability {
        public string skill;
        public string asset;
        public int amount;
        public int damage;
        public string type;
        public float spawnDistance;
        public float cooldown;
        public float startSkill;
        public float delay;
    }

    /// <summary>
    /// Main stats of enemy
    /// </summary>
    [Serializable]    
    public class EnemyInfo
    {
        public string name;
        public int health;
        public float speed;
        public int boss;
        public int levelMultiplyHp;
        public int maxKnockback = 3;
        public Loot[] loots;
        public string target;
        public float stopDistance;
        public string behavior;
        public int damage;        
        public float shootInterval;
        public Ability[] abilities;
        public int canRespawn;
    }

    /// <summary>
    /// Call this to get all enemies and the stats and loots of each
    /// </summary>
    [Serializable]
    public class Enemy
    {        
        public EnemyInfo[] enemy;
    }

    
}