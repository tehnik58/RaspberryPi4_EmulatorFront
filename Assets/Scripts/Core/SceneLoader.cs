using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// �������� �������� ���� � ���������� ������ ��������
/// ������������ ������� �������� ����� �������
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;  // ������ �� ������ ������ ��������
    [SerializeField] private float minimumLoadTime = 1.0f;  // ����������� ����� ������ ������ ��������

    private string _currentSceneName;  // ��� ������� ����������� �����

    /// <summary>
    /// �������� ����� �� �����
    /// </summary>
    /// <param name="sceneName">��� ����� ��� ��������</param>
    public void LoadScene(string sceneName)
    {
        if (_currentSceneName == sceneName) return;  // ���� ����� ��� ���������, �������

        StartCoroutine(LoadSceneCoroutine(sceneName));  // ��������� �������� ��������
    }

    /// <summary>
    /// �������� ��� ����������� �������� �����
    /// </summary>
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // ���������� ����� ��������
        if (loadingScreen != null) loadingScreen.SetActive(true);

        EventSystem.TriggerStatusMessage($"Loading {sceneName}...");

        float startTime = Time.time;  // ���������� ����� ������ ��������

        // ��������� ������� �����, ���� ��� ����������
        if (!string.IsNullOrEmpty(_currentSceneName))
        {
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(_currentSceneName);
            while (!unloadOperation.isDone)
            {
                yield return null;  // ���� ���������� ��������
            }
        }

        // ��������� ����� ����� ����������
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadOperation.allowSceneActivation = false;  // ��������� �������������� ���������

        // ���� ���������� �������� � ������������ ������� ������ ������ ��������
        while (loadOperation.progress < 0.9f || (Time.time - startTime) < minimumLoadTime)
        {
            yield return null;
        }

        loadOperation.allowSceneActivation = true;  // ��������� ��������� �����

        // ���� ������ �������� �����
        while (!loadOperation.isDone)
        {
            yield return null;
        }

        // ������������� ����������� ����� ��� ��������
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        _currentSceneName = sceneName;

        // �������� ����� ��������
        if (loadingScreen != null) loadingScreen.SetActive(false);

        EventSystem.TriggerStatusMessage($"{sceneName} loaded successfully");
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