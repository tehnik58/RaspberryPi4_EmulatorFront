using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Менеджер загрузки сцен с поддержкой экрана загрузки
/// Обеспечивает плавные переходы между сценами
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;  // Ссылка на объект экрана загрузки
    [SerializeField] private float minimumLoadTime = 1.0f;  // Минимальное время показа экрана загрузки

    private string _currentSceneName;  // Имя текущей загруженной сцены

    /// <summary>
    /// Загрузка сцены по имени
    /// </summary>
    /// <param name="sceneName">Имя сцены для загрузки</param>
    public void LoadScene(string sceneName)
    {
        if (_currentSceneName == sceneName) return;  // Если сцена уже загружена, выходим

        StartCoroutine(LoadSceneCoroutine(sceneName));  // Запускаем корутину загрузки
    }

    /// <summary>
    /// Корутина для асинхронной загрузки сцены
    /// </summary>
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // Показываем экран загрузки
        if (loadingScreen != null) loadingScreen.SetActive(true);

        EventSystem.TriggerStatusMessage($"Loading {sceneName}...");

        float startTime = Time.time;  // Запоминаем время начала загрузки

        // Выгружаем текущую сцену, если она существует
        if (!string.IsNullOrEmpty(_currentSceneName))
        {
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(_currentSceneName);
            while (!unloadOperation.isDone)
            {
                yield return null;  // Ждем завершения выгрузки
            }
        }

        // Загружаем новую сцену асинхронно
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadOperation.allowSceneActivation = false;  // Запрещаем автоматическую активацию

        // Ждем завершения загрузки и минимального времени показа экрана загрузки
        while (loadOperation.progress < 0.9f || (Time.time - startTime) < minimumLoadTime)
        {
            yield return null;
        }

        loadOperation.allowSceneActivation = true;  // Разрешаем активацию сцены

        // Ждем полной загрузки сцены
        while (!loadOperation.isDone)
        {
            yield return null;
        }

        // Устанавливаем загруженную сцену как активную
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        _currentSceneName = sceneName;

        // Скрываем экран загрузки
        if (loadingScreen != null) loadingScreen.SetActive(false);

        EventSystem.TriggerStatusMessage($"{sceneName} loaded successfully");
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