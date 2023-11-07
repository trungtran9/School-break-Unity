using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SB.Data { 
public class HeroData {
        public static Characters ReadHeroData(string path)
        {
            TextAsset heroAsset = Resources.Load<TextAsset>(path);
            return JsonUtility.FromJson<Characters>(heroAsset.text);
        }
    }
}