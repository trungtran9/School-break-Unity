using System;
using System.Collections.Generic;
using UnityEngine;

namespace SB.Data
{

    [Serializable]
    public class Hero
    {
        public string name;
        public string model;
        public string description;
        public string key;
        public int health;
        public float speed = 6.5f;        
        public float baseReloadTime;
        public string startWeapon;
        #region Bonus 
        public float health_Bonus;
        public float healPerSecond_Bonus;
        public float attack_Bonus;
        public float cooldown_Bonus;
        public float attackSpeed_Bonus;
        public float speed_Bonus;        
        public float duration_Bonus;
        public float area_Bonus;
        public float exp_Bonus;
        public int projectile_Bonus;
        #endregion
    }

    [Serializable]
    public class Characters
    {
        public Hero[] hero;

    }

}