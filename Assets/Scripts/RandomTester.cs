using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTester : MonoBehaviour
{
    private int[] dist;

    // Start is called before the first frame update
    void Start()
    {
        dist = new int[5];
    }

    // Update is called once per frame
    void Update()
    {
        float randomNumber = UnityEngine.Random.Range(0f, 10f);

        if (randomNumber <= 2f)
        { //if 1
          //flip sign of weight
            dist[0]++;
        }
        else if (randomNumber <= 4f)
        { //if 2
          //pick random weight between -1 and 1
            dist[1]++;
        }
        else if (randomNumber <= 6f)
        { //if 3
          //randomly increase by 0% to 100%
            float factor = UnityEngine.Random.Range(0f, 1f) + 1f;
            dist[2]++;
        }
        else if (randomNumber <= 8f)
        { //if 4
          //randomly decrease by 0% to 100%
            dist[3]++;
        }
        else
        {
            dist[4]++;
        }

        Debug.Log("1:" + dist[0].ToString() + " 2:" + dist[1].ToString() + " 3:" + dist[2].ToString() + " 4:" + dist[3].ToString() + " 5:" + dist[4].ToString());
    }
}
