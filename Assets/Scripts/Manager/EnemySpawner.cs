using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SB.Data;

public class EnemySpawner : MonoBehaviour
{
    protected enum SpawningState
    {        
        SPAWNING,
        RESPAWN,
        FINISH
    }

    private const int k_limit = 200;
    //Temp
    [SerializeField] GameObject Station;
    private GameObject stationContainer;
    private float stationSpawnPercent = 10;
    [SerializeField] WorldEvent.World worldEvents;
    [SerializeField] List<EnemyInfo> enemyInfos;
    [SerializeField] private int currentEventIndex;
    [SerializeField] GameObject enemyContainer;
    
    float timer;
    [SerializeField] private Camera cam;
    [SerializeField] SpawningState state;
    Queue<GameObject> enemyWaitQueue;
    
    // Start is called before the first frame update
    void Start()
    {
        //Read all the events
        worldEvents = WorldEventData.ReadWorldEventData("Json/WorldEvent").worldEvents[0];
        enemyInfos = new List<EnemyInfo>();
        enemyInfos.AddRange(EnemyData.ReadEnemyData("Json/Enemy").enemy);
        currentEventIndex = 0;
        timer = 0f;
        state = SpawningState.FINISH;
        enemyWaitQueue = new Queue<GameObject>();
        stationContainer = GameObject.FindGameObjectWithTag("StationContainer");
        InvokeRepeating("SpawnStation", 1f, 1f);
    }


    /// <summary>
    /// Spawn all the enemies in the current event
    /// </summary>
    IEnumerator SpawnEnemies()
    {
        state = SpawningState.SPAWNING;
        //First we delete all the previous enemies inside the pool
        foreach (Transform child in enemyContainer.transform)
            if (!child.gameObject.activeInHierarchy) Destroy(child.gameObject);
            else if (child.GetComponent<EnemyRanged>() != null) child.GetComponent<EnemyRanged>().CanDelete();
        enemyWaitQueue = new Queue<GameObject>();
        //Load the enemy GO Prefab
        foreach (WorldEvent.EnemySet enemySet in worldEvents.events[currentEventIndex].enemySets)
        {
            GameObject enemyToSpawn = Resources.Load<GameObject>("Enemy/" + enemySet.enemy);
            //Then spawn        
            for (int i = 0; i < enemySet.maxEnemy; i++)
            {
                GameObject enemy = Instantiate(enemyToSpawn);
                PositionEnemy(enemy);
                foreach (EnemyInfo enemyInfo in enemyInfos)
                {
                    if (enemyInfo.name.Equals(enemySet.enemy))
                    {
                        enemy.GetComponent<EnemyRanged>().Initialize(enemyInfo);
                        break;
                    }
                }                
                yield return new WaitForSeconds(worldEvents.events[currentEventIndex].interval);
            }
        }
        state = SpawningState.FINISH;
        currentEventIndex++;
    }

    void SpawnStation()
    {
        if (Random.Range(0, 100) < stationSpawnPercent)
        {
            if (stationContainer.transform.childCount < 10)
            {
                GameObject stationClone = Instantiate(Station);
                stationClone.GetComponent<EnemyRanged>().Initialize(enemyInfos.Find(x => x.name.Contains("Station")));
                PositionEnemy(stationClone);
                stationClone.transform.parent = stationContainer.transform;
                stationSpawnPercent = 10;
            }
        }
        else
        {
            stationSpawnPercent++;
        }
    }

    void PositionEnemy(GameObject enemy)
    {
        float height = cam.orthographicSize;
        float width = cam.orthographicSize * cam.aspect + 1;
        int horizontalOrVertical = Random.Range(0,2);
        
        if (horizontalOrVertical.Equals(0))
        {
            enemy.transform.position = new Vector3(cam.transform.position.x + Random.Range(-width, width),
                cam.transform.position.y + height * (Random.Range(0, 2) * 2 - 1) * 1.3f);
        }
        else
        {
            enemy.transform.position = new Vector3(cam.transform.position.x + width * (Random.Range(0, 2) * 2 - 1) * 1.3f,
                cam.transform.position.y + Random.Range(-height, height));
        }
        
        enemy.SetActive(true);
        enemy.transform.parent = enemyContainer.transform;
    }

    public void AddEnemyToQueue(GameObject enemy)
    {
        enemyWaitQueue.Enqueue(enemy);
    }

    IEnumerator HandleEnemies()
    {
        if (state.Equals(SpawningState.FINISH) || state.Equals(SpawningState.SPAWNING))
        {
            if (enemyWaitQueue.Count > 0)
            {                
                if (state.Equals(SpawningState.FINISH)) state = SpawningState.RESPAWN;
                GameObject enemy = enemyWaitQueue.Dequeue();
                PositionEnemy(enemy);
                if (state.Equals(SpawningState.RESPAWN)) yield return new WaitForSeconds(worldEvents.events[currentEventIndex - 1].interval);
                else yield return new WaitForSeconds(worldEvents.events[currentEventIndex].interval);
                if (state.Equals(SpawningState.RESPAWN)) state = SpawningState.FINISH;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentEventIndex <= worldEvents.events.Length - 1 && timer >= worldEvents.events[currentEventIndex].startTime)
        {
            if (state == SpawningState.FINISH)
                StartCoroutine(SpawnEnemies());
        }        
        if (enemyWaitQueue.Count > 0)
        {
            StartCoroutine(HandleEnemies());
        }
        timer += Time.deltaTime;
    }

    public EnemyInfo GetEnemyInfo(string name)
    {
        return enemyInfos.Find(x => x.name.Equals(name));
    }
}
