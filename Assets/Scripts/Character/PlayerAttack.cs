using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SB.Data;

/// <summary>
/// Handles player attack and calculating functions for projectiles
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    private const int k_limit = 5;
    private CapsuleCollider2D hitBox;
    public float bulletSpeed;
    public float health, attack;
    Vector2 movePos;
    //public GameObject bullet;
    public GameObject shootPoint;
    [SerializeField] List<GameObject> bullets;
    List<(float, bool)> reloadBulletTime;
    [SerializeField] List<Buff> bonusList;
    BulletSpawner bulletSpawner;
    PlayerMove playerMove;
    // Start is called before the first frame update
    void Start()
    {
        hitBox = GetComponent<CapsuleCollider2D>();                
        bulletSpawner = GameObject.FindGameObjectWithTag("BulletSpawner").GetComponent<BulletSpawner>();
        playerMove = GetComponent<PlayerMove>();
    }
    
    public void Initialize(Projectiles.Bullet bulletInfo)
    {
        bullets = new();
        bonusList = new();
        reloadBulletTime = new();
        ReadBulletInfo(bulletInfo);
    }

    GameObject ReadBulletInfo(Projectiles.Bullet bulletInfo)
    {
        GameObject bullet = Resources.Load<GameObject>("Projectiles/" + bulletInfo.name);
        Bullet bulletConfig = bullet.GetComponent<Bullet>();
        bulletInfo.level = 1;
        bulletConfig.SetBulletInfo(bulletInfo);
        bullets.Add(bullet);
        reloadBulletTime.Add((1.5f, false));
        return bullet;
    }

    void ReadBonusInfo(Bonus.BonusInfo bonusInfo)
    {
        Buff newBuff = new();
        newBuff.SetBonusInfo(bonusInfo);
        newBuff.SetLevel(1);
        bonusList.Add(newBuff);
        CalculateBuff(bonusInfo);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        StartCoroutine(CheckShoot());
    }

    IEnumerator CheckShoot()
    {
        for (int j = 0; j < bullets.Count; j++)
        {
            Projectiles.Bullet bulletInfo = bullets[j].GetComponent<Bullet>().bulletInfo;
            (float, bool) reload = reloadBulletTime[j];
            if (reload.Item1 <= 0f)
            {
                if (reload.Item2.Equals(false))
                {
                    reload.Item2 = true;
                    reloadBulletTime[j] = reload;                    
                    yield return StartCoroutine(bulletSpawner.SpawnProjectile(bullets[j], bulletInfo, this));                    
                    reload.Item1 = bulletInfo.cooldown - GetCooldownBonus();
                    if (bulletInfo.ignoreDuration.Equals(0)) reload.Item1 += bulletInfo.duration;
                    reload.Item2 = false;
                    reloadBulletTime[j] = reload;
                }
            }
            else
            {
                reload.Item1 -= Time.deltaTime;
                reloadBulletTime[j] = reload;
            }            
        }
    }

    public Transform GetClosestEnemy()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 10.0f);
        Collider2D nearest = null;
        if (colliders.Length == 0) return null;
        float distance = Mathf.Infinity;
        foreach (Collider2D collider in colliders)
        {
            if ((transform.position - collider.transform.position).sqrMagnitude < distance && collider.CompareTag("Enemy") && collider.GetComponent<EnemyRanged>() != null)
            {
                distance = (transform.position - collider.transform.position).sqrMagnitude;
                nearest = collider;
            }
        }
        return (nearest != null ? nearest.transform : null);
    }    

    public Vector3 GetShootPoint()
    {
        return new Vector3(shootPoint.transform.position.x, shootPoint.transform.position.y);
    }

    public Collider2D GetCollider()
    {
        return hitBox;
    }

    public List<Projectiles.Bullet> GetBulletInfo()
    {
        List<Projectiles.Bullet> infos = new ();
        foreach (GameObject bullet in bullets)
        {
            infos.Add(bullet.GetComponent<Bullet>().bulletInfo);
        }
        return infos;
    }

    public List<Bonus.BonusInfo> GetBonusInfo()
    {
        List<Bonus.BonusInfo> infos = new();
        foreach (Buff bonus in bonusList)
        {
            infos.Add(bonus.GetBonusInfo());
        }
        return infos;
    }

    public bool IsWeaponFullyUpgraded(string bulletName)
    {
        foreach (GameObject bullet in bullets)
        {
            Projectiles.Bullet bulletInfo = bullet.GetComponent<Bullet>().bulletInfo;
            if (bulletInfo.name.Equals(bulletName))
            {               
                return (bulletInfo.IsLevelMaxed());
            }                
        }
        return false;
    }

    public bool ReceiveBuff(Projectiles.Bullet bullet)
    {
        if (bullet != null)
        {
            foreach (GameObject b in bullets)
            {
                Bullet bInfo = b.GetComponent<Bullet>();
                if (bInfo.GetBulletInfo().name.Equals(bullet.name))
                {
                        bInfo.LevelUp();
                        bInfo.bulletInfo.RefreshDescription();
                        return true;
                }
            }
            ReadBulletInfo(bullet);
        }
        return false;
    }

    public void ReceiveBuff(Bonus.BonusInfo bonus)
    {
        if (bonus != null)
        {
            Buff bonusToFind = bonusList.Find(x => x.GetBonusInfo().name.Equals(bonus.name));
            if (bonusToFind == null)
            {
               ReadBonusInfo(bonus);
            }            
            else
            {                
                bonusToFind.LevelUp();
                bonusToFind.GetBonusInfo().RefreshDescription();
                CalculateBuff(bonusToFind.GetBonusInfo());
            }
        }
    }

    void CalculateBuff(Bonus.BonusInfo bonus)
    {
        Hero heroInfo = playerMove.GetHeroInfo();
        heroInfo.health_Bonus += bonus.baseHpChange;
        heroInfo.attackSpeed_Bonus += bonus.baseAttackSpeedChange;
        heroInfo.projectile_Bonus += bonus.baseProjectileChange;
        heroInfo.exp_Bonus += bonus.baseExperienceChange;
        heroInfo.duration_Bonus += bonus.baseDurationChange;
        heroInfo.speed_Bonus += bonus.baseMoveSpeedChange;
        heroInfo.cooldown_Bonus += bonus.baseCooldownChange;
        heroInfo.healPerSecond_Bonus += bonus.baseHPSChange;
    }

    #region Getters Setters
    public int GetProjectileBonus()
    {
        return playerMove.GetHeroInfo().projectile_Bonus;
    }

    public float GetDamageBonus()
    {
        return playerMove.GetHeroInfo().attack_Bonus;
    }

    public float GetSpeedBonus()
    {
        return playerMove.GetHeroInfo().speed_Bonus;
    }    

    public float GetAttackSpeedBonus()
    {
        return playerMove.GetHeroInfo().attackSpeed_Bonus;
    }

    public float GetCooldownBonus()
    {
        return playerMove.GetHeroInfo().cooldown_Bonus;
    }

    public bool GetLimitBonus()
    {
        return bonusList.Count.Equals(k_limit);
    }

    public bool GetLimitWeapon()
    {
        return bullets.Count.Equals(k_limit);
    }
    #endregion
}