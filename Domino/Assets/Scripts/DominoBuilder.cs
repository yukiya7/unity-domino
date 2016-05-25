using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DominoBuilder: MonoBehaviour {

    // 入力.

    public Vector3[] positions = null;
    public int position_num = 0;

    private GameObject first_domino;
    private GameObject[] dominos;

    private GameObject domino_obj;


    void Start()
    {
        domino_obj = Resources.Load("domino") as GameObject;
    }
    public GameObject createDomino()
    {
        dominos = new GameObject[position_num];
        for (int i = 0; i < position_num; i++)
        {
            GameObject o = Instantiate(domino_obj) as GameObject;
            o.transform.position = new Vector3(positions[i].x, .51f, positions[i].z);
            dominos[i] = o;
            if (i == 0) first_domino = o;

            if (i != position_num - 1)
            {
                o.transform.LookAt(new Vector3(positions[i + 1].x, .51f, positions[i + 1].z));
            }
            else
            {
                o.transform.LookAt(new Vector3(positions[i - 1].x, .51f, positions[i - 1].z));
            }

        }
        return first_domino;
    }

    public void RemoveDominos()
    {
        if (dominos != null)
        {
            Debug.Log("Remove Dominos.");
            for (int i = 0; i < dominos.Length; i++)
            {
                Destroy(dominos[i]);
            }
        }

    }
}
