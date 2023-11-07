using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SB.Data
{
    public static class WorldEventData
    {
        public static WorldEvent ReadWorldEventData(string path)
        {
            TextAsset worldAsset = Resources.Load<TextAsset>(path);
            return JsonUtility.FromJson<WorldEvent>(worldAsset.text);
        }

        public static void CreateWorldData(WorldEvent worldAsset)
        {
            JsonUtility.ToJson(worldAsset);
        }
    }
}