using UnityEngine;

public class UICharacterView : MonoBehaviour
{
    public GameObject[] characters;

    private int currentCharacter = 0;

    public int CurrectCharacter
    {
        get { return currentCharacter; }
        set
        {
            currentCharacter = value;
            this.UpdateCharacter();
        }
    }

    private void Start()
    {
    }

    private void Update()
    {
    }

    private void UpdateCharacter()
    {
        for (int i = 0; i < 3; i++)
        {
            characters[i].SetActive(i == this.currentCharacter);
        }
    }
}
