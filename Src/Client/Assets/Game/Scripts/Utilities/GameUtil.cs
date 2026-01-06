using UnityEngine;

class GameUtil : Singleton<GameUtil>
{
    public bool InScreen(Vector3 position)
    {
        return Screen.safeArea.Contains(Camera.main.WorldToScreenPoint(position));
    }
}
