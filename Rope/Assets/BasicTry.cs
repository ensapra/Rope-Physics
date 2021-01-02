using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTry : MonoBehaviour
{
    List<GameObject> gameObjects = new List<GameObject>();
    List<Vector3> previous = new List<Vector3>();
    public GameObject prefab;
    public GameObject BIGoNE;
    public GameObject BIGoNE2;
    public float distance;
    public float gravityVector;
    void Start()
    {
        for(int i = 0; i < 5; i++)
        {
            GameObject temp = Instantiate(prefab);
            gameObjects.Add(temp);
            previous.Add(temp.transform.position);
        }
    }
    void FixedUpdate()
    {
        //Inertia
        for(int i = 0; i < gameObjects.Count; i++)
        {
            GameObject current = gameObjects[i];
            Vector3 old = current.transform.position;
            current.transform.position += (current.transform.position-previous[i]);
            previous[i] = old;
        }

        //Gravity
        for(int i = 0; i < gameObjects.Count; i++)
        {
            GameObject current = gameObjects[i];
            current.transform.position += Vector3.down*gravityVector;
        }
        
        //Constraints
        for(int i = 0; i < gameObjects.Count; i++)
        {
            GameObject current = gameObjects[i];
            GameObject before;
            if(i == 0)
            {
                before = BIGoNE;
            }
            else
                before = gameObjects[i-1];
            current.transform.position = before.transform.position + (current.transform.position-before.transform.position).normalized*distance;
        }
        gameObjects.Reverse();
        for(int i = 1; i < gameObjects.Count; i++)
        {
            GameObject current = gameObjects[i];
            GameObject before;
                before = gameObjects[i-1];
            current.transform.position = before.transform.position + (current.transform.position-before.transform.position).normalized*distance;
        }
        gameObjects.Reverse();

    }
}
