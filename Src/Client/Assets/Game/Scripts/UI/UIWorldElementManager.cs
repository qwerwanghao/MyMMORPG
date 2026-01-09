using System.Collections.Generic;
using UnityEngine;
using Entities;

public class UIWorldElementManager : MonoSingleton<UIWorldElementManager>
{
    [SerializeField]
    private GameObject worldElementPrefab;
    private Dictionary<Transform, GameObject> worldElementPrefabs = new Dictionary<Transform, GameObject>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void AddCharacterElement(Transform playerTrans, Character character)
    {
        
        if (!worldElementPrefabs.ContainsKey(playerTrans))
        { 
            GameObject go = Instantiate(worldElementPrefab, this.transform);
            go.name = playerTrans.name + character.entityId;
            go.GetComponent<UIWorldElement>().player = playerTrans;
            go.GetComponent<UINameBar>().character = character;
            go.SetActive(true);
            worldElementPrefabs.Add(playerTrans, go);
        }
    }
    
    public void RemoveCharacterElement(Transform playerTrans)
    {
        if (worldElementPrefabs.ContainsKey(playerTrans))
        {
            GameObject go = worldElementPrefabs[playerTrans];
            go.SetActive(false);
            worldElementPrefabs.Remove(playerTrans);
        }
    }
}
