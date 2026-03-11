using UnityEngine;

public class TitleScreenUI : MonoBehaviour
{
    public void StartGame()
    {
        GameManager.Instance.StartGame();
        gameObject.SetActive(false);
    }
}
