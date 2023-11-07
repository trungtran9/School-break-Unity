using System;

namespace SB.Data
{
    [Serializable]
    public class WorldEvent
    {
        [Serializable]
        public class EnemySet
        {
            public string enemy;
            public int maxEnemy;
            public int repeat;
        }

        [Serializable]
        public class Event
        {
            public float startTime;
            public EnemySet[] enemySets;
            public float interval;            
        }

        [Serializable]
        public class World
        {
            public string name;
            public Event[] events;
        }

        public World[] worldEvents;
    }
}