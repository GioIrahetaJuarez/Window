using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Range(0f, 1f)]
    public float winClearPercent = 0.98f;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI progressText;
    public float messageDuration = 2f;

    Coroutine messageCoroutine;
    bool instructionShown = false;

    public bool IsGameOver { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this;
    }

    void Start()
    {
        ShowMessage("Left click and drag to clean the window\nUse mouse wheel to adjust brush size");
        instructionShown = true;
    }

    void Update()
    {
        if (IsGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    public void UpdateProgress(float percent)
    {
        if (progressText != null)
        {
            float displayPercent = Mathf.Clamp01(percent / winClearPercent);
            progressText.text = $"Progress: {Mathf.RoundToInt(displayPercent * 100)}%";
        }
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public void Win()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        Time.timeScale = 0f;
        ShowMessage("You Win!\nPress R to Restart");
    }

    public void Lose(string reason)
    {
        if (IsGameOver) return;
        IsGameOver = true;
        Time.timeScale = 0f;
        ShowMessage(reason + "\nPress R to Restart");
    }

    void ShowMessage(string text)
    {
        if (messageText == null) return;
        messageText.gameObject.SetActive(true);
        messageText.text = text;
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        
        if (!IsGameOver)
        {
            messageCoroutine = StartCoroutine(HideMessageAfter(messageDuration));
        }
    }

    IEnumerator HideMessageAfter(float sec)
    {
        yield return new WaitForSeconds(sec);
        if (messageText != null) messageText.gameObject.SetActive(false);
        messageCoroutine = null;
    }
}
