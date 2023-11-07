using UnityEngine;
using SB.Data;

/// <summary>
/// Main buff class. Consists of data read from json file.
/// Is used to get/set data, because buff doesn't have much functions besides its data.
/// </summary>
[System.Serializable]
public class Buff
{
    //The data from json class
    [SerializeField] private Bonus.BonusInfo bonusData;    
    
    //Leveling up buff will get the new description, although it could be the same as the original one.
    public void LevelUp()
    {
        bonusData.level++;      
    }

    //Returns the data of the buff. Can be used to add to the current hero's stats
    public Bonus.BonusInfo GetBonusInfo()
    {
        return bonusData;
    }

    //Sets the data of the buff    
    public void SetBonusInfo(Bonus.BonusInfo bonus)
    {
        bonusData = bonus;
    }

    //Set the level of this buff. In real context the level is always 1, but we can use this to test the effects.
    public void SetLevel(int level)
    {
        bonusData.level = level;
    }
}
