using UnityEngine;
using SB.Data;
using DG.Tweening;

/// <summary>
/// Bullet class. Gets bullet information from data and implement projectile behaviors.
/// </summary>
public class Bullet : MonoBehaviour
{
    #region Owner info
    [SerializeField]
    private GameObject bulletOwner;
    #endregion

    #region Bullet info
    public Projectiles.Bullet bulletInfo;
    #endregion
    public float initialSpeed;
    protected Vector3 dir;
    private Vector3 desiredDistance;
    private int currentLevel;
    [SerializeField] GameObject explosion;
    protected Rigidbody2D rb;
    protected float timer;
    protected int damage;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();       
        //rb.velocity = Vector2.zero;
        currentLevel = 0;
    }

    public virtual void Initialize()
    {
        Invoke(nameof(DestroyProjectile), bulletInfo.duration);
        timer = 0f;
        explosion = Resources.Load("Projectiles/" + bulletInfo.explosion) as GameObject;
        if (bulletInfo.bulletType.Equals("bounce"))
        {
            transform.DOJump(new Vector2(bulletOwner.transform.position.x + bulletInfo.baseRadius * (Random.Range(0, 2) * 2 - 1), bulletOwner.transform.position.y), 3.5f, 4, bulletInfo.duration, false)
                .SetEase(Ease.Flash).OnComplete(() =>
                {
                    AudioHandler.instance.PlaySingle(Resources.Load<AudioClip>("Sound/" + bulletInfo.shootSound));
                });
        }
        else {
            AudioHandler.instance.PlaySingle(Resources.Load<AudioClip>("Sound/" + bulletInfo.shootSound));            
        }
    }

    public void SetBulletOwner(GameObject bulletOwner, int damage)
    {
        this.bulletOwner = bulletOwner;
        desiredDistance = transform.position - bulletOwner.transform.position;
        if (bulletInfo.target == "self") transform.parent = bulletOwner.transform;
        this.damage = (int) damage;
    }

    // Update is called once per frame
    void Update()
    {
        CheckBulletType();
        timer += Time.deltaTime;
    }
    
    void CheckBulletType()
    {
        switch (bulletInfo.bulletType)
        {
            case "single":
                switch (bulletInfo.target)
                {
                    case "self":
                        transform.position = bulletOwner.transform.position + desiredDistance;
                        transform.RotateAround(bulletOwner.transform.position, new Vector3(0f, 0f, 1f), -(bulletInfo.projectileSpeed *
                            (1f + bulletOwner.GetComponent<PlayerAttack>().GetAttackSpeedBonus())) * 10 * Time.deltaTime);
                        desiredDistance = transform.position - bulletOwner.transform.position;
                        break;
                    case "directional":
                    case "closest":
                        if (Mathf.Abs(rb.velocity.magnitude) == 0) rb.velocity = dir * bulletInfo.projectileSpeed * (1f + bulletOwner.GetComponent<PlayerAttack>().GetAttackSpeedBonus());
                        if (bulletInfo.friction > 0) rb.velocity -= rb.velocity * bulletInfo.friction;

                        break;                    
                }
                break;
            case "taunt":
                Taunt();
                break;
        }
    }

    public void LevelUp()
    {
        bulletInfo.level++;
        bulletInfo.baseAmount += bulletInfo.levelup[bulletInfo.level - 2].projectileAmount;
        bulletInfo.minBaseDamage += bulletInfo.levelup[bulletInfo.level - 2].dmgChange;
        bulletInfo.maxBaseDamage += bulletInfo.levelup[bulletInfo.level - 2].dmgChange;
        bulletInfo.cooldown -= bulletInfo.levelup[bulletInfo.level - 2].cooldownChange;
        bulletInfo.baseRadius += bulletInfo.levelup[bulletInfo.level - 2].radiusChange;
        bulletInfo.scale += bulletInfo.levelup[bulletInfo.level - 2].scaleChange;
        bulletInfo.projectileSpeed += bulletInfo.levelup[bulletInfo.level - 2].speedChange;
        bulletInfo.duration += bulletInfo.levelup[bulletInfo.level - 2].durationChange;
    }

    protected void DestroyProjectile()
    {
        //Explosion handle
        if (explosion != null)
        {
            Explosion explosionClone = Instantiate(explosion, transform.position, Quaternion.identity).GetComponent<Explosion>();
            explosionClone.transform.localScale *= (1f + bulletInfo.scale);
            explosionClone.SetBulletInfo(bulletInfo);
            explosionClone.SetBulletOwner(bulletOwner, damage);
            AudioHandler.instance.PlaySingle(Resources.Load<AudioClip>("Sound/" + bulletInfo.explosionSound));
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && bulletInfo.basePenetrationTimes > 0)
        {
            bulletInfo.basePenetrationTimes--;
            //Should we make explosion here then?            
            if (bulletInfo.basePenetrationTimes == 0)
            {
                DestroyProjectile();
            }
        }
    }

    #region Specific skills
    private void Taunt()
    {
        Collider2D[] enemies = new Collider2D[100];
        Physics2D.OverlapCircleNonAlloc(transform.position, bulletInfo.baseRadius, enemies);
        foreach (Collider2D enemy in enemies)
        {
            if (enemy != null && enemy.CompareTag("Enemy"))
            {
                enemy.GetComponent<EnemyRanged>().SetTarget(transform);
            }
        }
    }


    #endregion

    #region Getters & Setters
    public Projectiles.Bullet GetBulletInfo()
    {
        return bulletInfo;
    }

    public void SetBulletInfo(Projectiles.Bullet bulletInfo)
    {
        this.bulletInfo = bulletInfo;
    }

    public void SetDirection(Vector2 dir)
    {
        this.dir = dir;
    }

    public Vector2 GetDirection()
    {
        return dir;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public virtual int GetDamage()
    {
       return damage;
    }

    public string GetBuff()
    {
        return bulletInfo.buffApply;
    }

    public float GetRemainingTime()
    {
        return bulletInfo.duration - timer;
    }
    #endregion
}
