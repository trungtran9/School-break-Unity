using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SB.Data;
using System.Collections;

/// <summary>
/// Handles general AI of the enemy
/// </summary>
public class EnemyRanged : MonoBehaviour
{
    class BulletList
    {
        public GameObject bullet;
        public float cd;

        public BulletList(GameObject bullet, float cd)
        {
            this.bullet = bullet;
            this.cd = cd;
        }
    }
    private bool invincible;
    public AudioClip hitSFX;
    private bool facingRight;
    public int health;
    private float speed;
    [SerializeField] private EnemyInfo enemyInfo;    
    public CapsuleCollider2D hitBox;
    private bool canDelete;

    private Transform Player;
    [SerializeField] private Transform target;
    [SerializeField] private Animator anim;
    [SerializeField] EnemySpawner spawner;
    private readonly List<GameObject> skillAsset = new();
    [SerializeField] AudioClip shootSFX;
    private readonly List<float> skillCooldown = new();
    private Camera cam;
    List<BulletList> bulletLists;
    Rigidbody2D rb;
    bool hit;    
    private bool isPosChanged;
    Vector2 tempDir;
   
    void Start()
    {
        hitBox = GetComponent<CapsuleCollider2D>();        
        spawner = GameObject.FindGameObjectWithTag("GameController").GetComponent<EnemySpawner>();
        // Resources.Load("Projectiles/enemyBullet") as GameObject;
        anim = GetComponent<Animator>();
        if (anim.runtimeAnimatorController != null) anim.SetBool("isWalking", true);
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        canDelete = false;
        bulletLists = new List<BulletList>();
        rb = GetComponent<Rigidbody2D>();
        hit = false;
        tempDir = Vector2.zero;
        invincible = false;        
    }

    /// <summary>
    /// Initializes this enemy AI with the data
    /// </summary>
    /// <param name="enemyInfo">data of this AI</param>
    public void Initialize(EnemyInfo enemyInfo)
    {
        Player = GameObject.FindGameObjectWithTag("Player").transform;
        SetTarget(Player);
        this.enemyInfo = enemyInfo;
        health = enemyInfo.levelMultiplyHp > 0 ? enemyInfo.health : Player.GetComponent<PlayerMove>().GetLevel() * enemyInfo.levelMultiplyHp;
        speed = enemyInfo.speed;
        if (enemyInfo.target.Equals("direction"))
        {
            if (CalculateMoveVector().x < 0) Flip();
        }
        if (enemyInfo.abilities != null)
        foreach (Ability ability in enemyInfo.abilities)
        {
            skillCooldown.Add(0f);
            if (ability.type.Equals("bullet"))
                skillAsset.Add(Resources.Load("Projectiles/" + ability.asset) as GameObject);
            else if (ability.type.Equals("summon"))
            {
                skillAsset.Add(Resources.Load("Enemy/" + ability.asset) as GameObject);
            }
        }        
    }

    // Update is called once per frame
    void Update()
    {
        if (target.Equals(null)) SetTarget(Player);
        //Handles movement                
        
        isPosChanged = CheckPos();
        if (!hit)
            switch (enemyInfo.target)
            {
                case "player":
                    rb.velocity = CalculateMoveVector().normalized * speed;
                    if (Vector2.Distance(transform.position, target.position) < enemyInfo.stopDistance && (target.Equals(Player))) rb.velocity = Vector2.zero;
                    break;
                case "direction":
                    if (isPosChanged || tempDir.Equals(Vector2.zero)) tempDir = CalculateMoveVector();
                    rb.velocity = tempDir.normalized * speed;
                    break;
            }

        //Handles sprite flip
        if (!enemyInfo.behavior.Equals("station") && !enemyInfo.target.Equals("direction"))
        {
            if (!facingRight && -CalculateMoveVector().x > 0)
            {
                Flip();
            }
            else if (facingRight && -CalculateMoveVector().x < 0)
            {
                Flip();
            }
        }

        //Handles attacking bullets based on their delays 
        foreach (BulletList bullet in bulletLists.Reverse<BulletList>())
        {
            bullet.cd -= Time.deltaTime;
            if (bullet.cd <= 0f) bulletLists.Remove(bullet);
        }
        
        CheckSkill();
    }

    /// <summary>
    /// Check the current position of enemy. Will move if enemy stays too far from player.
    /// </summary>
    /// <returns>true if enemy needs to be moved, false if nothing happens</returns>
    private bool CheckPos()
    {
        if (Vector2.Distance(transform.position, target.position) > cam.orthographicSize * cam.aspect * 3f)
        {
            if (enemyInfo.canRespawn.Equals(1))
            {
                transform.position = (Vector2)(target.position) + CalculateMoveVector();
            }
            else
            {
                Destroy(gameObject);
            }
            return true;

        }
        return false;
    }

    //Flip the player's sprite.
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;

    }

    //Get the desired move vector of enemy
    private Vector2 CalculateMoveVector()
    {
        return (target.position - transform.position);
    }

    void CheckSkill()
    {
        for (int i = 0; i < skillCooldown.Count; i++)
        {
            if (skillCooldown[i] <= 0f && Vector2.Distance(transform.position, target.position) <= enemyInfo.abilities[i].startSkill)
            {
                skillCooldown[i] = enemyInfo.abilities[i].cooldown;
                StartCoroutine(Fire(i));                
            }
            else if (skillCooldown[i] > 0f)
            {
                skillCooldown[i] -= Time.deltaTime;
            }
        }
    }

    IEnumerator Fire(int i)
    {
        Vector3 playerPos = Player.position;      
        for (int j = 0; j < enemyInfo.abilities[i].amount; j++)
        {
            var Asset = Instantiate(skillAsset[i], new Vector2(transform.position.x + Random.Range(-enemyInfo.abilities[i].spawnDistance, enemyInfo.abilities[i].spawnDistance),
                transform.position.y + Random.Range(-enemyInfo.abilities[i].spawnDistance, enemyInfo.abilities[i].spawnDistance)), Quaternion.identity);

            if (enemyInfo.abilities[i].type.Equals("bullet"))
            {
                Rigidbody2D bulletRb = Asset.GetComponent<Rigidbody2D>();
                //Guarantee z position of direction Vector is the same, else it can affect the constant position
                playerPos.z = transform.position.z;

                Asset.GetComponent<EnemyBullet>().SetDirection((playerPos - transform.position).normalized);

                Asset.GetComponent<EnemyBullet>().SetBulletOwner(gameObject, enemyInfo.abilities[i].damage);
                Asset.GetComponent<EnemyBullet>().Initialize();
                //float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;                
                AudioHandler.instance.PlaySingle(shootSFX);

                Physics2D.IgnoreCollision(Asset.GetComponent<CapsuleCollider2D>(), hitBox);
            }

            else if (enemyInfo.abilities[i].type.Equals("summon"))
            {
                EnemyInfo summonType = GameManager.instance.GetEnemyInfo(enemyInfo.abilities[i].asset);
                Asset.GetComponent<EnemyRanged>().Initialize(summonType);
                Physics2D.IgnoreCollision(Asset.GetComponent<CapsuleCollider2D>(), hitBox);
            }
            yield return new WaitForSeconds(enemyInfo.abilities[i].delay);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Projectile") && !Contain(other.gameObject) && !invincible)
        {
            bulletLists.Add(new BulletList(other.gameObject, other.gameObject.GetComponent<Bullet>().bulletInfo.impactCooldown));
            StartCoroutine(Hurt(other.gameObject));
            //Send to Status script                        
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Projectile") && !Contain(collision.gameObject) && !invincible)
        {
            bulletLists.Add(new BulletList(collision.gameObject, collision.gameObject.GetComponent<Bullet>().bulletInfo.impactCooldown));
            StartCoroutine(Hurt(collision.gameObject));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Projectile")) {
            EnemyBuff.BuffInfo buffInfo = GameManager.instance.GetEnemyBuffInfo(collision.GetComponent<Bullet>().GetBuff());
            if (buffInfo != null)
            switch (buffInfo.buffType)
            {
                case "staying":
                    speed = enemyInfo.speed;
                    break;
            }
        }
    }

    //
    public bool Contain(GameObject bullet)
    {
        return bulletLists.Find(x => x.bullet.Equals(bullet)) != null;
    }

    /// <summary>
    /// Perform hurt coroutine of enemy. Will be knocked back and unable to move for 0.1 second.
    /// If enemy's health is reduced to 0, this one is done.
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    IEnumerator Hurt(GameObject bullet)
    {
        Bullet bulletChosen = bullet.GetComponent<Bullet>();
        Vector2 difference = transform.position - Player.position;
        hit = true;
        if (!enemyInfo.target.Equals("direction")) rb.velocity = Vector2.zero;
        health -= bulletChosen.GetDamage();
        GetComponent<FlashColor>().Flash(Color.white);
        //Status - later
        EnemyBuff.BuffInfo buffInfo = GameManager.instance.GetEnemyBuffInfo(bulletChosen.GetBuff());
        if (buffInfo != null)
            switch (buffInfo.buffType)
            {
                case "staying": if (speed.Equals(enemyInfo.speed)) speed += enemyInfo.speed * buffInfo.baseSpeed;
                    break;
            }        
        if (health <= 0)
        {
            if (!IsBoss()) rb.AddRelativeForce(difference.normalized * bulletChosen.bulletInfo.knockbackForce, ForceMode2D.Impulse);
            hitBox.enabled = false;
            yield return new WaitForSeconds(0.2f);
            if (canDelete || IsBoss())
            {
                Destroy(gameObject);
                yield return null;
            }
            if (!enemyInfo.target.Equals("direction")) rb.velocity = Vector2.zero;
            health = enemyInfo.health;
            hitBox.enabled = true;
            spawner.AddEnemyToQueue(gameObject);
            SpawnLoot();
            hit = false;            
            GetComponent<FlashColor>().Deflash();
            gameObject.SetActive(false);
            GameManager.instance.ParseScore();
        }
        else
        {
            
            if (bulletLists.Count <= enemyInfo.maxKnockback)
                rb.AddForce(difference * bulletChosen.bulletInfo.knockbackForce, ForceMode2D.Impulse);
            invincible = true;
            yield return new WaitForSeconds(0.1f);
            invincible = false;
            rb.velocity = Vector2.zero;
            hit = false;
            AudioHandler.instance.PlaySingle(hitSFX);

        }

    }

    /// <summary>
    /// Spawns loot as weighted random
    /// </summary>
    private void SpawnLoot()
    {
        //Returns right away if no loot on enemy
        if (enemyInfo.loots == null) return;

        //Calculate the sum of loots' weights
        int max = 0;
        foreach (Loot loot in enemyInfo.loots)
        {
            max += loot.percent;
        }
        //Get a random number from 0 inclusive to sum weight ex
        int weight = Random.Range(0, max);

        //Iterate the loot array, if weight < loot's weight then we pick this loot, otherwise subtract its weight and continue
        foreach (Loot loot in enemyInfo.loots)
        {
            if (weight <= loot.percent)
            {
                if (loot.name.Equals("none")) return;
                GameObject pickup = Instantiate(Resources.Load<GameObject>("Pickups/" + loot.name), transform.position, Quaternion.identity);
                if (pickup.name.Contains("Exp"))
                    pickup.transform.parent = GameObject.FindGameObjectWithTag("ExpContainer").transform;
                break;
            }
            else weight -= loot.percent;
        }
    }

    #region Getter & Setters
    public int GetDamage()
    {
        return enemyInfo.damage;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public EnemyInfo GetEnemyInfo()
    {
        return enemyInfo;
    }

    public void SetEnemyInfo(EnemyInfo enemyInfo)
    {
        this.enemyInfo = enemyInfo;
    }

    //Return true if this enemy the big enemy (boss). Grants more exp/items when slain (further features)
    public bool IsBoss()
    {
        return (enemyInfo.boss.Equals(1));
    }

    public void CanDelete()
    {
        canDelete = true;
    }

    #endregion    

}