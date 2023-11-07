using System;

namespace SB.Data
{
    [Serializable]
    public class EnemyBuff
    {
        [Serializable]
        public class BuffInfo
        {
            public string name;
            public float baseSpeed;
            public string buffType;
            public string baseBuffDuration;
            public string baseTick;
            public int dps;
        }

        public BuffInfo[] buffs;
    }
}