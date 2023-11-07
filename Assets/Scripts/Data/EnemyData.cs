using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SB.Data
{
    public static class EnemyData
    {
        public static Enemy ReadEnemyData(string path)
        {
            TextAsset enemyAsset = Resources.Load<TextAsset>(path);
            return JsonUtility.FromJson<Enemy>(enemyAsset.text);
        }

        public static void CreateEnemyData(Enemy enemyAsset)
        {
            JsonUtility.ToJson(enemyAsset);
        }
    }
}