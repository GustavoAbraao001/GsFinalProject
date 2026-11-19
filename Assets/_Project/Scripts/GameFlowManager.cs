
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameOverReason
{
    NoExtinguishers,
    FireOutOfControl
}

public sealed class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }
    public static bool IsGameOver { get; private set; }

    [Header("Cenas")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Tela de Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text messageLabel;
    [SerializeField] private string title = "GAME OVER";

    [SerializeField, TextArea(2, 4)]
    private string message = "Os extintores acabaram.";

    [SerializeField, TextArea(2, 4)]
    private string fireOutOfControlMessage = "O incêndio saiu do controle.";

    [SerializeField, Min(0f)]
    private float buttonsDelaySeconds = 2f;

    [SerializeField] private GameObject buttonsContainer;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Parar o gameplay")]
    [Tooltip("Arraste os scripts de controle/interação do robô e os BabyPatrols.")]
    [SerializeField] private Behaviour[] behavioursToDisable;

    [Tooltip("Arraste também os NavMeshAgents dos NPCs.")]
    [SerializeField] private NavMeshAgent[] agentsToStop;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Debug.LogError(
                "Há mais de um GameFlowManager na cena de gameplay.",
                this
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;
        IsGameOver = false;
        Time.timeScale = 1f;

        if (gameOverPanel)
            gameOverPanel.SetActive(false);

        if (buttonsContainer)
            buttonsContainer.SetActive(false);

        if (titleLabel)
            titleLabel.text = title;

        if (messageLabel)
            messageLabel.text = message;

        if (restartButton)
            restartButton.onClick.AddListener(RestartGameplay);

        if (mainMenuButton)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void OnDestroy()
    {
        if (restartButton)
            restartButton.onClick.RemoveListener(RestartGameplay);

        if (mainMenuButton)
            mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);

        if (Instance == this)
        {
            Instance = null;
            IsGameOver = false;
        }
    }

    // Este método continua reservado para a situação de falta de extintores.
    public void TriggerGameOver()
    {
        TriggerGameOver(GameOverReason.NoExtinguishers);
    }

    public void TriggerGameOver(GameOverReason reason)
    {
        if (IsGameOver)
        {
            Debug.LogWarning(
                $"Game Over já estava ativo. Motivo ignorado: {reason}.",
                this
            );

            return;
        }

        IsGameOver = true;
        Time.timeScale = 0f;

        string selectedMessage = GetMessage(reason);

        Debug.Log(
            $"GAME OVER INICIADO | Motivo: {reason} | Mensagem: {selectedMessage}",
            this
        );

        if (ArcadeScoreManager.Instance)
            ArcadeScoreManager.Instance.SetPassiveScoringEnabled(false);

        StopGameplayBehaviours();

        if (titleLabel)
            titleLabel.text = title;

        if (messageLabel)
            messageLabel.text = selectedMessage;

        if (gameOverPanel)
            gameOverPanel.SetActive(true);
        else
            Debug.LogError(
                "GameFlowManager não tem Game Over Panel atribuído.",
                this
            );

        if (buttonsContainer)
        {
            buttonsContainer.SetActive(false);
            StartCoroutine(RevealButtonsAfterDelay());
        }
        else
        {
            Debug.LogWarning(
                "GameFlowManager não tem Buttons Container atribuído.",
                this
            );
        }
    }

    private string GetMessage(GameOverReason reason)
    {
        switch (reason)
        {
            case GameOverReason.FireOutOfControl:
                return fireOutOfControlMessage;

            case GameOverReason.NoExtinguishers:
            default:
                return message;
        }
    }

    public void RestartGameplay()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        LoadScene(currentSceneName);
    }

    public void ReturnToMainMenu()
    {
        LoadScene(mainMenuSceneName);
    }

    private IEnumerator RevealButtonsAfterDelay()
    {
        yield return new WaitForSecondsRealtime(buttonsDelaySeconds);

        if (buttonsContainer)
            buttonsContainer.SetActive(true);
    }

    private void StopGameplayBehaviours()
    {
        if (behavioursToDisable != null)
        {
            foreach (Behaviour behaviour in behavioursToDisable)
            {
                if (behaviour && behaviour != this)
                    behaviour.enabled = false;
            }
        }

        if (agentsToStop != null)
        {
            foreach (NavMeshAgent agent in agentsToStop)
            {
                if (agent && agent.enabled && agent.isOnNavMesh)
                    agent.isStopped = true;
            }
        }
    }

    private static void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError(
                "O nome da cena que será carregada está vazio."
            );

            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }
}
