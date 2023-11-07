using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SB.Data;

/// <summary>
/// Helps spawning bullets from player each time player wants to call bullet spawning.
/// Get infos from bulletInfo parameter, which is read from json.
/// </summary>
public class BulletSpawner : MonoBehaviour
{
    public AudioClip shootSFX;
    [SerializeField] private Camera cam;  

    /// <summary>
    /// Spawn projectiles of a bullet type.
    /// </summary>
    /// <param name="bullet"></param>
    /// <param name="bulletInfo"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public IEnumerator SpawnProjectile(GameObject bullet, Projectiles.Bullet bulletInfo, PlayerAttack player)
    {
        string target = bulletInfo.target;
        //Directional, single
        float offset = (bulletInfo.baseAmount + player.GetProjectileBonus() - 1) * 0.5f;
        float v = (bulletInfo.baseAmount + player.GetProjectileBonus() - 1) * 10 / 2;
        //Self, single
        float degDifference = 2 * Mathf.PI / (bulletInfo.baseAmount + player.GetProjectileBonus());
        Vector2 dir = player.GetComponent<PlayerMove>().GetMovePos();

        if (target != "closest" || (target == "closest" && player.GetClosestEnemy() != null))
            for (int i = 0; i < bulletInfo.baseAmount + player.GetProjectileBonus(); i++)
            {
                var BulletSpawn = Instantiate(bullet, player.GetShootPoint(), transform.rotation);
                //AudioHandler.instance.PlaySingle(shootSFX);
                BulletSpawn.transform.localScale *= (1f + bulletInfo.scale);
                Bullet bulletComp = BulletSpawn.GetComponent<Bullet>();                                
                bulletComp.SetDirection(dir);
                
                switch (target)
                {
                    case "self":
                        BulletSpawn.transform.position = new Vector2(BulletSpawn.transform.position.x + bulletInfo.baseRadius * Mathf.Cos((i + 1) * degDifference),
                        BulletSpawn.transform.position.y + bulletInfo.baseRadius * Mathf.Sin((i + 1) * degDifference));
                        break;
                    case "directional":
                        if (bulletInfo.spread.Equals(1))
                        {
                            //BulletSpawn.transform.Rotate(new Vector3(0f, 0f, v));
                            bulletComp.SetDirection(new Vector2(dir.x * Mathf.Cos(-v * Mathf.PI / 180) + dir.y * Mathf.Sin(-v * Mathf.PI / 180),
                                                                -dir.x * Mathf.Sin(-v * Mathf.PI / 180) + dir.y * Mathf.Cos(-v * Mathf.PI / 180)));
                            v = (i - offset) * 10;
                        }
                        break;
                    case "random":
                        BulletSpawn.transform.position = GetRandomPointAroundPlayer(player.transform.position, bulletInfo.baseRadius);
                        break;
                    case "closest":
                        Transform closestEnemy = player.GetClosestEnemy();
                        if (closestEnemy != null)
                        {
                            dir = (closestEnemy.position - player.transform.position).normalized;
                            
                        }
                        else
                        {
                            dir = new Vector2(Random.Range(0, 2) * 2 - 1, Random.Range(0, 2) * 2 - 1).normalized;
                        }
                        bulletComp.SetDirection(dir);
                        break;
                }

                Rigidbody2D bulletRb = BulletSpawn.GetComponent<Rigidbody2D>();
                //bulletRb.drag = bulletInfo.friction;
                if (bulletInfo.tangentForce > 0f)
                {
                    bulletRb.AddForce((Random.Range(0, 2) * 2 - 1) * bulletInfo.tangentForce * Vector2.Perpendicular(dir), ForceMode2D.Impulse);
                    yield return new WaitForSeconds(0.2f);
                }

                bulletComp.SetBulletOwner(player.gameObject, (int) (Random.Range(bulletInfo.minBaseDamage, bulletInfo.maxBaseDamage) * (1 + player.GetDamageBonus())));
                bulletComp.Initialize();
                
                Physics2D.IgnoreCollision(BulletSpawn.GetComponent<Collider2D>(), player.GetCollider());
                if (bulletInfo.bulletType.Equals("taunt") && i < 1) break;
                yield return new WaitForSeconds(bulletInfo.delay);
            }
    }

    /// <summary>
    /// Calculate a random point on screen. Suitable for multiple random bullets
    /// </summary>
    /// <returns> Vector2 value of the point</returns>
    private Vector2 GetRandomPointOnScreen()
    {
        float height = cam.orthographicSize;
        float width = cam.orthographicSize * cam.aspect + 1;
        return new Vector2(cam.transform.position.x + Random.Range(-width / 2, width / 2), cam.transform.position.y + Random.Range(-height / 2, height / 2));
    }

    private Vector2 GetRandomPointAroundPlayer(Vector2 player, float radius)
    {
        return new Vector2(player.x + Random.Range(- radius, radius), player.y + Random.Range(- radius, radius));
    }
}
