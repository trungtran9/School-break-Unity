using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using SB.Data;
using DG.Tweening;
using System.Collections;
using UnityEngine.GameFoundation.Components;
using UnityEngine.GameFoundation.DefaultLayers;
using UnityEngine.GameFoundation;
using UnityEngine.Promise;

/// <summary>
/// Handles the main gameflow. The jobs of GameManager basically are:
/// - Time management from UI interactions: Pause/Unpause, Leveling up, Defeat scene.
/// - Parsing data.
/// - Load scene between main menu and main game scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    //UI
    [Header("UI Components")]
    public GameObject gameOverPanel;
    public GameObject nextWavePanel;
    [SerializeField] Text timerText;

    //Pause Panel
    [Header("Pause")]
    public GameObject pausePanel;
    [SerializeField] TextMeshProUGUI txtKill;
    [SerializeField] TextMeshProUGUI txtLevel;
    [SerializeField] GameObject pauseButton;

    [Header("Controller components")]
    public GameFoundationInit gameFoundationInit;
    public Cinemachine.CinemachineVirtualCamera cam;
    [SerializeField] SpaceElement renderSpace;
    private BuffHandler buffHandler;
    private ScoringSystem scoringSystem;


    public Text winText;
    private Hero heroInfo;
    private PlayerMove player;
    private static float Timer = 0f;
    
    // Start is called before the first frame update

    #region Unity Callbacks
    void Start()
    {
        scoringSystem = GetComponent<ScoringSystem>();        
        gameOverPanel.transform.localScale = Vector3.zero;
        nextWavePanel.transform.localScale = Vector3.zero;
        pausePanel.transform.localScale = Vector3.zero;
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        AudioHandler.instance.PlayMusic();
        winText.gameObject.SetActive(false);

        // Reset and reinitialize Game Foundation
        gameFoundationInit.Uninitialize();

        // Begin Game Foundation initialization process
        gameFoundationInit.Initialize();
        Timer = 1f;        
    }
    
    public void Initialize(Hero heroData)
    {
        heroInfo = heroData;
        GameObject playerPrefab = Resources.Load<GameObject>("Characters/" + heroData.name);        
        GameObject playerClone = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        cam.Follow = playerClone.transform;
        renderSpace.SetPlayer(playerClone);

        buffHandler = GetComponent<BuffHandler>();
        buffHandler.Initialize();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
        player.Initialize(heroInfo);
        player.GetComponent<PlayerAttack>().Initialize(GetFirstBuff(heroInfo.startWeapon));

    }
    void Update()
    {
        Timer += Time.deltaTime;
        int roundtime = Mathf.RoundToInt(Timer);
        timerText.text = (roundtime / 60).ToString() + ":" + (roundtime % 60 < 10 ? "0" + (roundtime % 60).ToString() : (roundtime % 60).ToString());
    }

    #endregion

    #region Game Flow Management
    public void ParseScore()
    {
        scoringSystem.passEnemyScore();
    }

    public void ChooseBuff()
    {
        Pause();
        buffHandler.GetAvailableBuff();
        nextWavePanel.transform.DOScale(1, 0.2f).SetEase(Ease.OutSine).SetUpdate(true);
    }

    public void PassBuff(Button card)
    {
        buffHandler.PassBuffToPlayer(card);
        nextWavePanel.transform.DOScale(0, 0.2f).SetEase(Ease.OutSine).SetUpdate(true);
        StartCoroutine(DisablePause());        
    }

    public void GameOver()
    {
        Pause();
        gameOverPanel.transform.DOScale(1, 0.2f).SetEase(Ease.OutSine).SetUpdate(true);
        gameOverPanel.transform.GetChild(2).GetComponent<Text>().text = scoringSystem.GetScore().ToString();
    }

    private void Pause()
    {
        Time.timeScale = 0f;
        pauseButton.GetComponent<Button>().interactable = false;
        player.ToggleJoystick();
    }

    public void PausePanel()
    {
        pausePanel.transform.DOScale(1, 0.2f).SetEase(Ease.OutSine).SetUpdate(true);
        txtLevel.text = player.GetLevel().ToString();
        txtKill.text = scoringSystem.GetScore().ToString();
        Pause();
    }    

    public void UnpausePanel()
    {
        pausePanel.transform.DOScale(0, 0.2f).SetEase(Ease.OutSine).SetUpdate(true);
        StartCoroutine(DisablePause());
    }

    IEnumerator DisablePause()
    {
        player.ToggleJoystick();
        yield return new WaitForSecondsRealtime(1f);
        UnPause();
    }

    public void UnPause()
    {        
        Time.timeScale = 1f;
        pauseButton.GetComponent<Button>().interactable = true;        
    }

    public void Reset()
    {
        scoringSystem.SaveScore();
        // Get the persistence data layer used during Game Foundation initialization.
        if (GameFoundationSdk.dataLayer is not PersistenceDataLayer dataLayer)
            return;

        using (Deferred saveOperation = dataLayer.Save()) { }
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void BackToHome()
    {
        if (GameFoundationSdk.dataLayer is not PersistenceDataLayer dataLayer)
            return;
        using (Deferred saveOperation = dataLayer.Save()) { }
        StartCoroutine(LoadMenu());
    }
    IEnumerator LoadMenu()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MenuScene", LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        SceneManager.UnloadSceneAsync("MainScene");
        Time.timeScale = 1f;
    }

    #endregion

    #region Getters & Setters
    public Projectiles.Bullet GetFirstBuff(string name)
    {
        return buffHandler.GetBullet(name);
    }

    public float GetTime()
    {
        return Timer;
    }

    public EnemyBuff.BuffInfo GetEnemyBuffInfo(string name)
    {
        return buffHandler.GetBuffInfo(name);
    }

    public EnemyInfo GetEnemyInfo(string name)
    {
        return GetComponent<EnemySpawner>().GetEnemyInfo(name);
    }
    #endregion

}