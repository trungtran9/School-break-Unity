using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScoringSystem : MonoBehaviour
{
    int kills;

    private void Awake()
    {
        kills = 0;
    }
    public void SaveScore()
    {
    }

    public void passEnemyScore()
    {
        kills++;
    }
    
    public int GetScore()
    {
        return kills;
    }
}
