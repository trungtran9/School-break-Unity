using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    [SerializeField] private Image weaponImage;
    private string weaponName = "";
    int level;
    [SerializeField] private TextMeshProUGUI txtLevel;
    // Start is called before the first frame update

    public void SetWeaponUI(Sprite sprite, string name)
    {
        weaponImage.sprite = sprite;
        weaponName = name;
        level = 1;
        txtLevel.text = level.ToString();
    }

    public void SetLevel()
    {
        level++;
        txtLevel.text = level.ToString();
    }

    public string GetWeaponName()
    {
        return weaponName;
    }
}
