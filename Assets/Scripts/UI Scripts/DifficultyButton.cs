using UnityEngine;
using UnityEngine.UI;

public class DifficultyButton : MonoBehaviour
{
    private Button button;
    private GameManager gameManager;
    public int difficulty;

    void Awake()
    {
        button = GetComponent<Button>();
        gameManager = FindFirstObjectByType<GameManager>();

        if (button != null)
        {
            button.onClick.AddListener(SetDifficulty);
        }
        else
        {
            Debug.LogError("DifficultyButton necesita un componente Button en el mismo GameObject.");
        }
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(SetDifficulty);
        }
    }

    /* When a button is clicked, call the StartGame() method
     * and pass it the difficulty value (1, 2, 3) from the button 
    */
    void SetDifficulty()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (gameManager == null)
        {
            Debug.LogError("No se encontro GameManager para iniciar el juego.");
            return;
        }

        Debug.Log(gameObject.name + " was clicked");
        gameManager.StartGame(difficulty);
    }
}
