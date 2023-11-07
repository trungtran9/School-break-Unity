using UnityEngine;
using SB.Data;

public static class EnemyBuffData
{
    public static EnemyBuff ReadEnemyBuffData(string path)
    {
        TextAsset bonusAsset = Resources.Load<TextAsset>(path);
        return JsonUtility.FromJson<EnemyBuff>(bonusAsset.text);
    }
}
