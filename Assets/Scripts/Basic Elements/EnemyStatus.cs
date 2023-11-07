using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SB.Data;

public class EnemyStatus : MonoBehaviour
{
    [SerializeField] List<EnemyBuff.BuffInfo> buff;

    private void Awake()
    {
        buff = new List<EnemyBuff.BuffInfo>();
    }
    public void GetDebuffFromBullet(Projectiles.Bullet bulletInfo)
    {
        buff.Add(GameManager.instance.GetEnemyBuffInfo(bulletInfo.buffApply));
    }

    public void RemoveDebuffFromBullet(Projectiles.Bullet bulletInfo)
    {
        buff.Remove(buff.Find(x => x.name.Equals(bulletInfo.buffApply)));
    }

}
