using UnityEngine;
using UnityEngine.UI;
using SB.Data;
using System.Collections.Generic;
using UnityEngine.GameFoundation;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{

    #region Infos
    //Constants
    private const float k_hurtTime = 0.5f;
    public int level = 1;

    public AudioClip hitSFX;

    private float baseHealth;
    public float health;
    public Image healthUI;
    public Image breakUI;
    private float speed;
    
    //Enemies that attack at the moment
    private List<(EnemyRanged, float)> enemiesAttacking;
    private string moveMode;
    #endregion

    #region Endless Exp Variables
    public AnimationCurve playerExpCurve;    
    private float currentExp;
    private float requiredExp;
    #endregion

    private float horizontal, vertical;
    Rigidbody2D body;
    private bool canMove = false;
    bool facingRight = true;
    Animator anim;
    private SpriteRenderer spriteRenderer;
    private Hero heroInfo;
    public FloatingJoystick joystick;
    public GameObject bullet;
    public Camera cam;
    private Currency currency;
    #region Weapon
    public GameObject weapon;
    public GameObject shootPoint;
    #endregion

    Vector3 mousePos;
    Vector2 movePos;

    private new ParticleSystem particleSystem;
    // Start is called before the first frame update
    void Start()
    {
        canMove = true;
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        particleSystem = GetComponent<ParticleSystem>();
        //
        currentExp = 0;      

        enemiesAttacking = new List<(EnemyRanged, float)>();
        currency = GameFoundationSdk.catalog.Find<Currency>("diamond");        
        Debug.Log("The new balance is " + GameFoundationSdk.wallet.Get(currency).ToString());
        InvokeRepeating("RegenHp", 1f, 1f);
    }

    public void Initialize(Hero heroInfo)
    {
        this.heroInfo = heroInfo;
        baseHealth = heroInfo.health;
        speed = heroInfo.speed;
        health = baseHealth;
        level = 1;
        requiredExp = playerExpCurve.Evaluate(level);
        GameObject component = GameObject.FindGameObjectWithTag("PlayerComponent");
        joystick = component.transform.GetChild(0).GetComponent<FloatingJoystick>();
        breakUI = component.transform.GetChild(1).GetChild(0).GetComponent<Image>();
        ShowBreakUI();
        ShowUI();
    }

    public void OnMove(InputValue value)
    {
        var v = value.Get<Vector2>();
        if (canMove)
        {
            horizontal = v.x;
            vertical = v.y;
        }
    }

    void Update()
    {
        if (canMove)
        {            
            horizontal = joystick.Horizontal;            
            vertical = joystick.Vertical;           
        }
        //mousePos = camera.ScreenToWorldPoint(Input.mousePosition);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (canMove)
        {
            HandleMovement(horizontal, vertical);

            for (int i = enemiesAttacking.Count - 1; i > 0; i--)
            {
                if (enemiesAttacking[i].Item1 == null)
                {
                    enemiesAttacking.RemoveAt(i);
                    continue;
                }
                (EnemyRanged, float) enemy = enemiesAttacking[i];
                enemy.Item2 -= Time.deltaTime;
                if (enemy.Item2 <= 0f)
                {
                    Hurt(enemy.Item1.GetDamage());
                    enemy.Item2 = k_hurtTime;
                }
                enemiesAttacking[i] = enemy;
            }
        }
    }

    //Move the player depending on horizontal + vertical values
    private void HandleMovement(float horizontal, float vertical)
    {
        var delta = mousePos - transform.position;

        if (facingRight && horizontal < 0)
        {
            Flip();
        }
        else if (!facingRight && horizontal > 0)
        {
            Flip();
        }

        if (horizontal != 0 && vertical != 0)
        {
            horizontal *= Mathf.Cos(Mathf.PI / 4);
            vertical *= Mathf.Cos(Mathf.PI / 4);
        }

        if (horizontal != 0 || vertical != 0)
        {
            anim.SetBool("isWalking", true);
            movePos = new Vector2(horizontal, vertical);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }
        body.velocity = new Vector2(horizontal * speed * (1 + heroInfo.speed_Bonus), vertical * speed * (1 + heroInfo.speed_Bonus));

    }

    //Flip the player's sprite.
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        spriteRenderer.flipX = !facingRight;
        //transform.localScale = theScale;
    }

    public void ReceiveExp(float exp)
    {
        currentExp += exp * (1 + heroInfo.exp_Bonus); 
        if (currentExp >= requiredExp)
        {
            //Level up, get a buff
            currentExp -= requiredExp;
            level++;            
            requiredExp = playerExpCurve.Evaluate(level);
            GameManager.instance.ChooseBuff();            
        }
        ShowBreakUI();
    }

    public void ReceiveHp(float hp)
    {        
        health += hp;
        if (health > baseHealth) health = baseHealth;
        ShowUI();
    }

    public void ReceiveDiamond(int diamond)
    {
        GameFoundationSdk.wallet.Add(currency, diamond);        
    }

    public void SetWeapon(GameObject weaponPrefab)
    {
        //this.weapon.GetComponent<SpriteRenderer>().sprite = weaponPrefab.GetComponent<SpriteRenderer>().sprite;
        this.bullet = weaponPrefab;

        canMove = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.GetComponent<EnemyBullet>() != null) Hurt(other.GetComponent<EnemyBullet>().GetDamage());
            else if (!other.GetComponent<EnemyRanged>().GetEnemyInfo().behavior.Equals("station"))
                enemiesAttacking.Add((other.GetComponent<EnemyRanged>(), 0f));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            foreach ((EnemyRanged, float) enemy in enemiesAttacking)
            {
                if (collision.GetComponent<EnemyRanged>().Equals(enemy.Item1))
                {
                    enemiesAttacking.Remove(enemy);
                    break;
                }
            }
        }
    }

    void Hurt(int attack)
    {
        AudioHandler.instance.PlaySingle(hitSFX);
        GetComponent<FlashColor>().Flash(Color.red);
        anim.SetTrigger("Hurt");
        health -= attack;
        ShowUI();
        if (health <= 0)
        {
            GameManager.instance.GameOver();
        }
    }

    private void RegenHp()
    {
        ReceiveHp(heroInfo.healPerSecond_Bonus);
    }

    public void SetCurrentSprite(Sprite sprite)
    {
       // particleSystem.textureSheetAnimation.SetSprite(0, sprite);
    }

    void ShowUI() {
        float healthPortion = (health / baseHealth);
        healthUI.fillAmount = healthPortion;        
    }

    void ShowBreakUI()
    {
        float portion = (currentExp / requiredExp);
        breakUI.fillAmount = portion;
    }

    public int GetLevel()
    {
        return level;
    }

    public Vector2 GetMovePos()
    {        
        return (movePos.Equals(Vector2.zero) ? new Vector2(1f, 0f) : movePos);
    }

    public void ToggleJoystick()
    {
        joystick.gameObject.SetActive(!joystick.gameObject.activeInHierarchy);
        joystick.ResetJoystick();
    }

    public void SetHealth(int health)
    {
        this.health += health;
        baseHealth += health;
        ShowUI();
    }
    public Hero GetHeroInfo()
    {
        return heroInfo;
    }
}