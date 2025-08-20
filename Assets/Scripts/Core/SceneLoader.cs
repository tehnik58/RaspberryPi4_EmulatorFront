using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// �������� �������� ���� � ���������� ������ ��������
/// ������������ ������� �������� ����� �������
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("UI References")]
    public GameObject loadingScreen;
    public float minimumLoadTime = 1.0f;

    private string _currentSceneName;

    /// <summary>
    /// ������������� Singleton ��� ��������
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
    /// �������� ����� �� �����
    /// </summary>
    /// <param name="sceneName">��� ����� ��� ��������</param>
    public void LoadScene(string sceneName)
    {
        if (_currentSceneName == sceneName) return;

        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    /// <summary>
    /// �������� ��� ����������� �������� �����
    /// </summary>
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // ���������� ����� ��������
        ShowLoadingScreen(true);
        EventSystem.TriggerStatusMessage($"Loading {sceneName}...");

        float startTime = Time.time;

        // ��������� ������� ����� ���� ��� ����������
        if (!string.IsNullOrEmpty(_currentSceneName))
        {
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(_currentSceneName);
            while (!unloadOperation.isDone)
            {
                yield return null;
            }
        }

        // ��������� ����� ����� ����������
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadOperation.allowSceneActivation = false;

        // ���� ���������� �������� � ������������ ������� ������
        while (loadOperation.progress < 0.9f || (Time.time - startTime) < minimumLoadTime)
        {
            yield return null;
        }

        loadOperation.allowSceneActivation = true;

        // ���� ������ �������� �����
        while (!loadOperation.isDone)
        {
            yield return null;
        }

        // ������������� ����������� ����� ��� ��������
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        _currentSceneName = sceneName;

        // �������� ����� ��������
        ShowLoadingScreen(false);
        EventSystem.TriggerStatusMessage($"{sceneName} loaded successfully");
    }

    /// <summary>
    /// ��������/������ ����� ��������
    /// </summary>
    private void ShowLoadingScreen(bool show)
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(show);
        }
    }

    /// <summary>
    /// �������� ����� �������� ����
    /// </summary>
    public void LoadMainMenu()
    {
        LoadScene("MainMenu");
    }

    /// <summary>
    /// �������� ����� ������������
    /// </summary>
    public void LoadConstructor()
    {
        LoadScene("Constructor");
    }
}