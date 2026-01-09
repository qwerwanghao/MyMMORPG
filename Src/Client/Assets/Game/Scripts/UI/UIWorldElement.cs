using UnityEngine;

public class UIWorldElement : MonoBehaviour
{
    public Transform player;
    public float height = 2.2f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            transform.position = player.position + Vector3.up * height;
            
        }
    }
}
