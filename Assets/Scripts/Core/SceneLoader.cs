using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Менеджер загрузки сцен с поддержкой экрана загрузки
/// Обеспечивает плавные переходы между сценами
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("UI References")]
    public GameObject loadingScreen;
    public float minimumLoadTime = 1.0f;

    private string _currentSceneName;

    /// <summary>
    /// Инициализация Singleton при создании
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Загрузка сцены по имени
    /// </summary>
    /// <param name="sceneName">Имя сцены для загрузки</param>
    public void LoadScene(string sceneName)
    {
        if (_currentSceneName == sceneName) return;

        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    /// <summary>
    /// Корутина для асинхронной загрузки сцены
    /// </summary>
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // Показываем экран загрузки
        ShowLoadingScreen(true);
        EventSystem.TriggerStatusMessage($"Loading {sceneName}...");

        float startTime = Time.time;

        // Выгружаем текущую сцену если она существует
        if (!string.IsNullOrEmpty(_currentSceneName))
        {
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(_currentSceneName);
            while (!unloadOperation.isDone)
            {
                yield return null;
            }
        }

        // Загружаем новую сцену асинхронно
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadOperation.allowSceneActivation = false;

        // Ждем завершения загрузки и минимального времени показа
        while (loadOperation.progress < 0.9f || (Time.time - startTime) < minimumLoadTime)
        {
            yield return null;
        }

        loadOperation.allowSceneActivation = true;

        // Ждем полной загрузки сцены
        while (!loadOperation.isDone)
        {
            yield return null;
        }

        // Устанавливаем загруженную сцену как активную
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        _currentSceneName = sceneName;

        // Скрываем экран загрузки
        ShowLoadingScreen(false);
        EventSystem.TriggerStatusMessage($"{sceneName} loaded successfully");
    }

    /// <summary>
    /// Показать/скрыть экран загрузки
    /// </summary>
    private void ShowLoadingScreen(bool show)
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(show);
        }
    }

    /// <summary>
    /// Загрузка сцены главного меню
    /// </summary>
    public void LoadMainMenu()
    {
        LoadScene("MainMenu");
    }

    /// <summary>
    /// Загрузка сцены конструктора
    /// </summary>
    public void LoadConstructor()
    {
        LoadScene("Constructor");
    }
}