using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceElement : MonoBehaviour
{
    private GameObject player;    
    //Test only
    private int size = 40;
    private int renderSize = 40;
    public GameObject firstChunk;
    public List<GameObject> chunkPrefabs;

    // Start is called before the first frame update
    void Start()
    {
        OpenChunk(firstChunk);
    }

    void OpenChunk(GameObject chosenPrefab)
    {
        foreach (Transform child in chosenPrefab.transform)
        {
            child.gameObject.SetActive(true);
        }        
        for (int i = -1; i < 2; i++)
            for (int j = -1; j < 2; j++)
            {
                if (Physics2D.OverlapCircle(new Vector2(chosenPrefab.transform.position.x + i * size, chosenPrefab.transform.position.y + j * size), 0.02f) == null)
                    {
                        GameObject chunk = Instantiate(chunkPrefabs[Random.Range(0, chunkPrefabs.Count)], new Vector2(chosenPrefab.transform.position.x + i * size, chosenPrefab.transform.position.y + j * size), Quaternion.identity);
                        chunk.transform.parent = transform;
                    }                
            }
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            for (int i = -1; i < 2; i++)
                for (int j = -1; j < 2; j++)
                {
                    if (Physics2D.OverlapCircle(new Vector2(player.transform.position.x + i * renderSize, player.transform.position.y + j * renderSize), 0f) != null
                            && Physics2D.OverlapCircle(new Vector2(player.transform.position.x + i * renderSize, player.transform.position.y + j * renderSize), 0f).CompareTag("FloorChunk"))
                        continue;
                    if (Physics2D.OverlapCircle(new Vector2(player.transform.position.x + (-i * 2) * renderSize, player.transform.position.y + j * renderSize), 0f) != null
                        && Physics2D.OverlapCircle(new Vector2(player.transform.position.x + (-i * 2) * renderSize, player.transform.position.y + j * renderSize), 0f).CompareTag("FloorChunk"))
                    {
                        GameObject chunk = Physics2D.OverlapCircle(new Vector2(player.transform.position.x - i * 2 * renderSize, player.transform.position.y + j * renderSize), 0f)
                            .gameObject;
                        chunk.transform.position = new Vector2(chunk.transform.position.x + i * 3 * size, chunk.transform.position.y);
                        break;
                    }

                    else if (Physics2D.OverlapCircle(new Vector2(player.transform.position.x + (-i * 2) * renderSize, player.transform.position.y + (-j * 2) * renderSize), 0f) != null
                        && Physics2D.OverlapCircle(new Vector2(player.transform.position.x + (-i * 2) * renderSize, player.transform.position.y + (-j * 2) * renderSize), 0f).CompareTag("FloorChunk"))
                    {
                        GameObject chunk = Physics2D.OverlapCircle(new Vector2(player.transform.position.x + -i * 2 * renderSize, player.transform.position.y + -j * 2 * renderSize), 0f)
                            .gameObject;
                        chunk.transform.position = new Vector2(chunk.transform.position.x + i * 3 * size, chunk.transform.position.y + j * 3 * size);
                        break;
                    }

                    else if (Physics2D.OverlapCircle(new Vector2(player.transform.position.x + i * renderSize, player.transform.position.y - j * 2 * renderSize), 0f) != null
                        && Physics2D.OverlapCircle(new Vector2(player.transform.position.x + i * renderSize, player.transform.position.y - j * 2 * renderSize), 0f).CompareTag("FloorChunk"))
                    {
                        GameObject chunk = Physics2D.OverlapCircle(new Vector2(player.transform.position.x + i * renderSize, player.transform.position.y + (-j * 2) * renderSize), 0f)
                            .gameObject;
                        chunk.transform.position = new Vector2(chunk.transform.position.x, chunk.transform.position.y + j * 3 * size);
                        break;
                    }
                }
        }
    }

    public void SetPlayer(GameObject player)
    {
        this.player = player;
    }
}