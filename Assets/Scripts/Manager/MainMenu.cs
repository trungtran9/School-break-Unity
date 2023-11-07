using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultLayers;
using UnityEngine.GameFoundation;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SB.Data;
using TMPro;
using System.Collections;
using UnityEngine.Promise;

public class MainMenu : MonoBehaviour
{
    public GameFoundationInit gameFoundationInit;
    [SerializeField] TextMeshProUGUI txtProgress;
    [SerializeField] GameObject btnHero;
    private Currency currency;
    List<Hero> heroes = new List<Hero>();
    Hero chosenHero;    
    [Header("Heroes Panel")]
    [SerializeField] TextMeshProUGUI txtDiamond;
    [SerializeField] GameObject heroesPanel;
    [SerializeField] GameObject characterSlot;
    
    private readonly List<InventoryItem> items = new ();
    // Start is called before the first frame update
    void Start()
    {
        GameFoundationSdk.inventory.GetItems(items);
        // Begin Game Foundation initialization process
        //gameFoundationInit.Initialize();
        currency = GameFoundationSdk.catalog.Find<Currency>("diamond");
        txtDiamond.text = GameFoundationSdk.wallet.Get(currency).ToString();        
        heroes.AddRange(HeroData.ReadHeroData("Json/Hero").hero);
        heroesPanel.transform.parent.gameObject.SetActive(false);
        LoadCharacter();
    }

    void LoadCharacter()
    {        
        foreach (Hero hero in heroes)
        {
            GameObject heroPanel = Instantiate(characterSlot, heroesPanel.transform);
            heroPanel.name = hero.key;
            heroPanel.GetComponent<Button>().onClick.AddListener(delegate { BuyCharacter(hero.key); });
            Sprite[] sprites = Resources.LoadAll<Sprite>("ver2/Character/" + hero.model);
            heroPanel.transform.GetChild(1).GetComponent<Image>().sprite = sprites[0];
            InventoryItemDefinition heroDef = GameFoundationSdk.catalog.Find<InventoryItemDefinition>(hero.key);

            if (items.Find(x => x.definition.key.Equals(hero.key)) != null)
            {
                heroPanel.transform.GetChild(1).GetComponent<Image>().color = Color.white;
                heroPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";                
            }
            else
            {
                heroPanel.transform.GetChild(1).GetComponent<Image>().color = Color.black;
                heroPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = heroDef.GetStaticProperty("requiredDiamond");
            }            
        }
        chosenHero = heroes[0];
    }

    public void StartGame()
    {         
        // Get the persistence data layer used during Game Foundation initialization.
        if (!(GameFoundationSdk.dataLayer is PersistenceDataLayer dataLayer))
            return;
        // - Deferred is a struct that helps you track the progress of an asynchronous operation of Game Foundation.
        // - We use a using block to automatically release the deferred promise handler.
        using (Deferred saveOperation = dataLayer.Save()) { }        
        StartCoroutine(LoadSceneGame(chosenHero));
    }

    IEnumerator LoadSceneGame(Hero hero)
    {        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);        
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            txtProgress.text = progress.ToString();
            yield return null;
        }        
        SceneManager.UnloadSceneAsync("MenuScene");
        GameManager.instance.Initialize(hero);        
    }

    public void BuyCharacter(string key)
    {
        InventoryItemDefinition heroDef = GameFoundationSdk.catalog.Find<InventoryItemDefinition>(key);
        if (items.Find(x => x.definition.key.Equals(key)) != null)
        {
            EquipPlayer(key);
            return;
        }
        if (heroDef.GetStaticProperty("requiredDiamond") <= GameFoundationSdk.wallet.Get(currency))
        {
            Debug.Log("can buy");
            GameObject hero = heroesPanel.transform.Find(key).gameObject;
            hero.transform.GetChild(1).GetComponent<Image>().color = Color.white;
            hero.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
            GameFoundationSdk.wallet.Set(currency, GameFoundationSdk.wallet.Get(currency) - heroDef.GetStaticProperty("requiredDiamond"));
            txtDiamond.text = GameFoundationSdk.wallet.Get(currency).ToString();
            GameFoundationSdk.inventory.CreateItem(heroDef);            
            EquipPlayer(key);
        }        
    }

    void EquipPlayer(string key)
    {
        chosenHero = heroes.Find(x => x.key.Equals(key));
        btnHero.GetComponent<Animator>().Play(chosenHero.model);
    }

    public void ResetProgress()
    {
        if (!(GameFoundationSdk.dataLayer is PersistenceDataLayer dataLayer))
            return;
        foreach (Transform child in heroesPanel.transform)
        {
            Destroy(child.gameObject);
        }
        GameFoundationSdk.wallet.Set(currency, 0);
        GameFoundationSdk.inventory.DeleteAllItems();
        GameFoundationSdk.inventory.CreateItem(GameFoundationSdk.catalog.Find<InventoryItemDefinition>("character1"));
        GameFoundationSdk.inventory.GetItems(items);
        LoadCharacter();
        using (Deferred saveOperation = dataLayer.Save()) { }
    }
}
