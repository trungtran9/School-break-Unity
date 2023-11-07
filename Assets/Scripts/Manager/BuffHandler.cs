using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SB.Data;
using TMPro;

/// <summary>
/// Handles buffs and weapons for player after each level up
/// </summary>
public class BuffHandler : MonoBehaviour
{
    public GameObject[] buffPanels;
    private readonly List<EnemyBuff.BuffInfo> enemyBuffs = new List<EnemyBuff.BuffInfo>();
    PlayerAttack player;
    [SerializeField] private readonly List<Projectiles.Bullet> existingBullets = new();
    [SerializeField] private readonly List<Bonus.BonusInfo> existingBonus = new();
    private List<PowerUp> currentBuffCards;
    [Header("WeaponUI")]
    [SerializeField] GameObject weaponList;
    [SerializeField] GameObject bonusList;
    [SerializeField] GameObject weaponImage;

    public void Initialize()
    {        
        enemyBuffs.AddRange(EnemyBuffData.ReadEnemyBuffData("Json/Enemy Buffs").buffs);        
        existingBullets.AddRange(ProjectilesData.ReadProjectilesData("Json/Bullet").bullet);
        existingBonus.AddRange(BonusData.ReadBonusData("Json/Bonus").bonus);
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAttack>();
        foreach (Transform child in weaponList.transform)
            Destroy(child.gameObject);
        foreach (Transform child in bonusList.transform)
            Destroy(child.gameObject);
    }

    /// <summary>
    /// Called first. When the level up label shows, get available buff from both players and buff pools
    /// </summary>
    public void GetAvailableBuff()
    {
        //Generate available bullets
        List<Projectiles.Bullet> availableBullets = CreateChooseableWeapons();
        List<Bonus.BonusInfo> availableBonus = CreateChooseableBuffs();
        currentBuffCards = new List<PowerUp>();
        foreach (GameObject card in buffPanels)
        {         
            if (availableBullets.Count <= 0 && availableBonus.Count <= 0)                 
            {
                AddVoidCard(card);
            }
            else
            {
                float bonusOrWeapon = UnityEngine.Random.Range(0, 100);
                if (bonusOrWeapon <= 20 && availableBonus.Count > 0)
                {                 
                    Bonus.BonusInfo chosen = Choose(availableBonus);
                    availableBonus.Remove(chosen);
                    AddInfoToCards(card, chosen);
                }
                else if (availableBullets.Count > 0)
                {
                    Projectiles.Bullet chosen = Choose(availableBullets);
                    availableBullets.Remove(chosen);
                    AddInfoToCards(card, chosen);
                }
            }
        }  
    }

    /// <summary>
    /// Initialization. Create the list of weapons to be chosen.
    /// </summary>
    /// <returns>A list of chosable weapons. If null, should return the gold/food buff</returns>
    List<Projectiles.Bullet> CreateChooseableWeapons()
    {
        List<Projectiles.Bullet> choosables = new();

        //Go through the owned Weapon first
        foreach (Projectiles.Bullet bullet in player.GetBulletInfo())
        {
            if (!player.IsWeaponFullyUpgraded(bullet.name))
            {
                choosables.Add(bullet);
            }
        }
        //Don't add available Weapon if player hits the limit
        if (!player.GetLimitWeapon())
            foreach (Projectiles.Bullet bullet in existingBullets)
            {       
                bool contain = false;
                foreach (Projectiles.Bullet bullet2 in choosables)
                    if (bullet2.name.Equals(bullet.name))
                    {
                        contain = true;
                        break;
                    }
                if (!contain) choosables.Add(bullet);
            }
        choosables.Sort((u, v) => v.weight.CompareTo(u.weight));
        return choosables;
    }

    /// <summary>
    /// Initialization. Create the list of buffs to be chosen.
    /// </summary>
    /// <returns>A list of chosable buffs. If null, should return the gold/food buff</returns>
    List<Bonus.BonusInfo> CreateChooseableBuffs()
    {
        List<Bonus.BonusInfo> choosables = new();
         
        //Go through the owned Buff first
        foreach (Bonus.BonusInfo bonus in player.GetBonusInfo())
        {
            if (!player.IsWeaponFullyUpgraded(bonus.name))
            {
                choosables.Add(bonus);
            }
        }
        //If bonus hits limit then we don't need to add available bonus
        if (!player.GetLimitBonus())
            foreach (Bonus.BonusInfo bonus in existingBonus)
            {
                bool contain = false;
                foreach (Bonus.BonusInfo bonus2 in choosables)
                    if (bonus2.name.Equals(bonus.name))
                    {
                        contain = true;
                        break;
                    }
                if (!contain) choosables.Add(bonus);
            }
        choosables.Sort((u,v) => v.weight.CompareTo(u.weight));
        return choosables;
    }

    void AddInfoToCards(GameObject card, Projectiles.Bullet chosen)
    {
        currentBuffCards.Add(chosen);
        card.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = chosen.level == 0 ? chosen.description : chosen.GetCurrentLevel().description;
        card.transform.Find("txtTitle").GetComponent<TextMeshProUGUI>().text = chosen.name;
        card.transform.Find("txtLevel").GetComponent<TextMeshProUGUI>().text = chosen.level == 0 ? "new" : "level " + (chosen.level + 1).ToString();
        card.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("weapon/" + chosen.name);
    }

    void AddInfoToCards(GameObject card, Bonus.BonusInfo chosen)
    {
        currentBuffCards.Add(chosen);
        Debug.Log("current level: " + chosen.level);
        card.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = chosen.level == 0 ? chosen.description : chosen.GetCurrentLevel().description;
        card.transform.Find("txtTitle").GetComponent<TextMeshProUGUI>().text = chosen.name;
        card.transform.Find("txtLevel").GetComponent<TextMeshProUGUI>().text = chosen.level == 0 ? "new" : "level " + (chosen.level + 1).ToString();
        card.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("weapon/" + chosen.name);
    }

    void AddVoidCard(GameObject card)
    {
        card.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = "Only choose if you don't have anything else";
        card.transform.Find("txtTitle").GetComponent<TextMeshProUGUI>().text = "Nothing";
        card.transform.Find("txtLevel").GetComponent<TextMeshProUGUI>().text = "";
        card.transform.Find("Image").GetComponent<Image>().enabled = false;
    }

    Projectiles.Bullet Choose(List<Projectiles.Bullet> buffArray)
    {
        return ChooseOneBuff(buffArray);
    }

    Projectiles.Bullet ChooseOneBuff(List<Projectiles.Bullet> bulletArray)
    {
        int sumWeight = 0;
        foreach (Projectiles.Bullet bullet in bulletArray)
        {
            sumWeight += bullet.weight;
        }
        int weight = UnityEngine.Random.Range(0, sumWeight);
        foreach (Projectiles.Bullet bullet in bulletArray)
        {
            if (weight < bullet.weight)
            {
                return bullet;
            }
            else weight -= bullet.weight;
        }
        return null;
    }

    Bonus.BonusInfo Choose(List<Bonus.BonusInfo> bonusArray)
    {
        int sumWeight = 0;
        foreach (Bonus.BonusInfo bonus in bonusArray)
        {
            sumWeight += bonus.weight;
        }
        int weight = UnityEngine.Random.Range(0, sumWeight);
        foreach (Bonus.BonusInfo bonus in bonusArray)
        {
            if (weight < bonus.weight)
            {
                return bonus;
            }
            else weight -= bonus.weight;
        }
        return null;
    }

    /// <summary>
    /// Uses in the beginning of the game. Find and remove the first weapon the character has.
    /// </summary>
    /// <param name="name">The bullet name of the player. Be careful of spelling.</param>
    /// <returns>The Bullet-info type of the bullet, which was removed from the list</returns>
    public Projectiles.Bullet GetBullet(string name)
    {
        foreach (Projectiles.Bullet bullet in existingBullets)
        {
            if (bullet.name.Equals(name)) {
                existingBullets.Remove(bullet);
                GameObject image = Instantiate(weaponImage, weaponList.transform);
                image.GetComponent<WeaponUI>().SetWeaponUI(Resources.Load<Sprite>("weapon/" + name), name);                                
                return bullet;
            }            
        }
        return null;
    }

    public Bonus.BonusInfo GetBonus(string name)
    {
        foreach (Bonus.BonusInfo bonus in existingBonus)
        {
            if (bonus.name.Equals(name))
            {
                existingBonus.Remove(bonus);
                GameObject image = Instantiate(weaponImage, bonusList.transform);
                image.GetComponent<WeaponUI>().SetWeaponUI(Resources.Load<Sprite>("weapon/" + name), name);
                return bonus;
            }
        }
        return null;
    }

    public EnemyBuff.BuffInfo GetBuffInfo(string name)
    {
        return enemyBuffs.Find(x => x.name.Equals(name));
    }

    /// <summary>
    /// Parse the chosen buff to player and add to player's buffs/weapons list.
    /// </summary>
    /// <param name="card">The chosen card of 3.</param>
    public void PassBuffToPlayer(Button card)
    {      
        for (int i = 0; i < card.transform.parent.childCount; i++)
        {
            if (card.transform == card.transform.parent.GetChild(i))
            {
                //Instantiate only if the weapon is found on card                
                if (currentBuffCards[i] is Projectiles.Bullet bullet) {                    
                    Projectiles.Bullet newBullet = GetBullet(currentBuffCards[i].name);
                    if (newBullet != null)
                    {
                        player.ReceiveBuff(newBullet);                    
                    }
                    else
                    {
                        player.ReceiveBuff(bullet);
                        foreach (Transform child in weaponList.transform)
                        {
                            if (child.GetComponent<WeaponUI>().GetWeaponName().Equals(bullet.name))
                            {
                                child.GetComponent<WeaponUI>().SetLevel();
                                break;
                            }                                
                        }
                    }
                }
                else
                {
                    Bonus.BonusInfo newBonus = GetBonus(currentBuffCards[i].name);
                    if (newBonus != null)
                    {
                        player.ReceiveBuff(newBonus);          
                    }
                    else
                    {
                        player.ReceiveBuff((Bonus.BonusInfo)currentBuffCards[i]);
                        foreach (Transform child in bonusList.transform)
                        {
                            if (child.GetComponent<WeaponUI>().GetWeaponName().Equals(currentBuffCards[i].name))
                            {
                                child.GetComponent<WeaponUI>().SetLevel();
                                break;
                            }                            
                        }
                    }
                }
                break;
            }
        }
    }
}