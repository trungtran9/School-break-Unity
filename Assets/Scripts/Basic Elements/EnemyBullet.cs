using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : Bullet
{
    float speed = 5f;
    int attack;
    public override void Initialize()
    {
        Invoke(nameof(DestroyProjectile), 4f);
        timer = 0f;        
    }
    private void Update()
    {
        rb.velocity = dir * speed;
        timer += Time.deltaTime;
    }
    public override int GetDamage()
    {
        return attack;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Projectile"))
        {
                DestroyProjectile();
        }
    }
}
