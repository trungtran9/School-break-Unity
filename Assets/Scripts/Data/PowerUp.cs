using System;

namespace SB.Data {

    [Serializable]
    public class PowerUp
    {
        public string name;
        public string description;
        public string model;
        public int weight = 10;
        public int level = 0;
    }
}