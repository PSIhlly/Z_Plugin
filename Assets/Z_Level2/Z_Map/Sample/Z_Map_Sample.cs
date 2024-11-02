using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Map;

public class Z_Map_Sample : MonoBehaviour
{
    public GameObject mapPre;
    public Material[] mats;
    public GameObject[] itemPres;
    public GameObject[] characterPres;
    public void Start()
    {

        Vector3[] itemPoss = new Vector3[2] { new Vector3(2, 0.2f, 2), new Vector3(1, 0, 1) };
        Vector3[] itemEulars = new Vector3[2] { new Vector3(0, 0, 0), new Vector3(1, 45, 1) };

        Vector3[] characterPoss = new Vector3[2] { new Vector3(3, 0.2f, 3), new Vector3(7, 0, 7) };
        Vector3[] characterEulars = new Vector3[2] { new Vector3(0, 0, 0), new Vector3(1, 45, 1) };


        MapUnit[,,] maps = new MapUnit[10,1, 10];

        for (int i = 0; i < maps.GetLength(0); i++)
        {
            for (int j = 0; j < maps.GetLength(1); j++)
            {
                for (int k = 0; k < maps.GetLength(2); k++)
                {
                    maps[i, j, k] = new MapUnit(k, mapPre,new Vector3(i, j, k),new Vector3(90,0,0), mats[(i+j+k)%3]);
                }
            }
        }
        var items = new ItemUnit[itemPoss.Length];
        for (int i = 0; i < itemPoss.Length; i++)
        {
            items[i] = new ItemUnit(i,itemPres[i],itemPoss[i],itemEulars[i],true);
        }

        var characters = new CharacterUnit[characterPoss.Length];
        for (int i = 0; i < characterPoss.Length; i++)
        {
            characters[i] = new CharacterUnit(i,characterPres[i], characterPoss[i], characterEulars[i]);
        }

        MapManager.instance.Begin(new Vector3Int(5, 1, 5),maps,items,characters);

    }
    public void Update()
    {
        MapManager.instance.SetPos(transform.position);
        MapManager.instance.characterDic[0].navEnabled = true;
        MapManager.instance.characterDic[0].destination = transform.position;
        MapManager.instance.characterDic[1].navEnabled = true;
        MapManager.instance.characterDic[1].destination = transform.position;
        if (Input.GetKey(KeyCode.W))
            transform.position += Time.deltaTime * Vector3.forward*2;
        if (Input.GetKey(KeyCode.S))
            transform.position += Time.deltaTime * Vector3.back*2;
        if (Input.GetKey(KeyCode.A))
            transform.position += Time.deltaTime * Vector3.left*2;
        if (Input.GetKey(KeyCode.D))
            transform.position += Time.deltaTime * Vector3.right*2;
    }
}
