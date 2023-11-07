using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] int exp;
    [SerializeField] int hp;
    [SerializeField] int diamond;
    [SerializeField] bool allCoins;
    float speed = 6f;
    float accel = 2f;
    [SerializeField] AudioClip collectSFX;
    Transform target;
    Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {            
            transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * speed);
            speed = speed + accel * Time.deltaTime;
            if (Vector3.Distance(transform.position, target.position) <= 0.2f)
            {
                GiveExp();
            }
        } 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerArea") && target == null)
        {
            StartCoroutine(Knockback(collision.transform));
            SetTarget(collision.transform);
            if (transform.childCount > 0)
                transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        }
    }

    IEnumerator Knockback(Transform target)
    {
        Vector2 difference = transform.position - target.position;
        rb.AddForce(difference.normalized * 15f, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.2f);
        rb.velocity = Vector2.zero;
    }

    void GiveExp()
    {        
        AudioHandler.instance.PlaySingle(collectSFX);
        if (exp > 0) target.parent.GetComponent<PlayerMove>().ReceiveExp(exp);
        if (hp > 0) target.parent.GetComponent<PlayerMove>().ReceiveHp(hp);
        if (diamond > 0) target.parent.GetComponent<PlayerMove>().ReceiveDiamond(1);
        if (allCoins) {
            GameObject container = GameObject.FindGameObjectWithTag("ExpContainer");
            foreach (Transform child in container.transform)
            {
                child.GetComponent<Pickup>().SetTarget(target);
            }
        }
        Destroy(gameObject);
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}
