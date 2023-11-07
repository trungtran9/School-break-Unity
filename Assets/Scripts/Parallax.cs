using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Camera cam;
    SpriteRenderer sprite;
    private float length, startposX, startPosY;
    public float parallaxScrolling;
    // Start is called before the first frame update
    void Start()
    {
        startposX = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float temp = cam.transform.position.x * (1 - parallaxScrolling);
        float dist = cam.transform.position.x * parallaxScrolling;
        transform.position = new Vector3(startposX + dist, transform.position.y, transform.position.z);
        if (temp > startposX + length) startposX += length;
        else if (temp < startposX - length) startposX -= length;
    }
}
