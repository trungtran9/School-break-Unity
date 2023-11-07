using UnityEngine;
using SB.Data;
public static class BonusData
{
    public static Bonus ReadBonusData(string path)
    {
        TextAsset bonusAsset = Resources.Load<TextAsset>(path);
        return JsonUtility.FromJson<Bonus>(bonusAsset.text);
    }
}
