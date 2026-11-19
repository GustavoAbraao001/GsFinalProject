
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PauseController : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("Interface")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    private float previousTimeScale = 1f;

    private void Awake()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (pausePanel)
            pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (GameFlowManager.IsGameOver)
            return;

        // Se Settings estiver aberto, o ESC não controla o Pause.
        if (IsSettingsOpen())
            return;

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private bool IsSettingsOpen()
    {
        return settingsPanel != null &&
               settingsPanel.activeInHierarchy;
    }

    public void TogglePause()
    {
        if (GameFlowManager.IsGameOver || IsSettingsOpen())
            return;

        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (GameFlowManager.IsGameOver || IsPaused)
            return;

        previousTimeScale = Time.timeScale;
        IsPaused = true;
        Time.timeScale = 0f;

        if (pausePanel)
            pausePanel.SetActive(true);
    }

    public void Resume()
    {
        if (GameFlowManager.IsGameOver || !IsPaused)
            return;

        Time.timeScale = previousTimeScale;
        IsPaused = false;

        if (pausePanel)
            pausePanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (!IsPaused)
            return;

        Time.timeScale = previousTimeScale;
        IsPaused = false;
    }
}
