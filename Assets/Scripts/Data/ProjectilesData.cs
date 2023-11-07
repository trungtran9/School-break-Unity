using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SB.Data
{
    public static class ProjectilesData
    {
        public static Projectiles ReadProjectilesData(string path)
        {
            TextAsset projectileAsset = Resources.Load<TextAsset>(path);
            return JsonUtility.FromJson<Projectiles>(projectileAsset.text);
        }

        public static void CreateProjectileData(Projectiles projectilesAsset)
        {
            JsonUtility.ToJson(projectilesAsset);
        }
    }
}
