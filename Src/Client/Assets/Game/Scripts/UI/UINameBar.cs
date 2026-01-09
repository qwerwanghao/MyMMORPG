// System references
using System.Text;

// Unity references
using UnityEngine;
using UnityEngine.UI;

// Project references
using Entities;

public class UINameBar : MonoBehaviour
{
    // Serialized fields
    [SerializeField]
    private Text playerNameText;

    // Public fields
    public Character character;

    // Private fields
    private int lastLevel = -1;
    private StringBuilder sb = new StringBuilder();
    private Camera cachedCamera;

    // Unity lifecycle
    void Start()
    {
        Refresh();

        cachedCamera = MainPlayerCamera.Instance?.followCamera ?? Camera.main;
    }

    void Update()
    {
        UpdateCharacterInfo();

        if (cachedCamera != null)
        {
            this.transform.forward = cachedCamera.transform.forward;
        }
    }

    // Public methods
    public void Refresh()
    {
        lastLevel = -1;
        UpdateCharacterInfo();
    }

    // Private methods
    void UpdateCharacterInfo()
    {
        if (character == null || playerNameText == null)
            return;

        if (character.Info.Level != lastLevel)
        {
            sb.Clear();
            sb.Append(character.Name);
            sb.Append(" Lv.");
            sb.Append(character.Info.Level);
            playerNameText.text = sb.ToString();
            lastLevel = character.Info.Level;
        }
    }
}
