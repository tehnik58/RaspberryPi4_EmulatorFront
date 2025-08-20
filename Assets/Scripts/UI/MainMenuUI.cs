using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ���������� ������� ���� � ����������
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Menu Sections")]
    public GameObject mainSection;
    public GameObject settingsSection;
    public GameObject aboutSection;

    [Header("Main Menu Buttons")]
    public ButtonHandler newProjectButton;
    public ButtonHandler loadProjectButton;
    public ButtonHandler settingsButton;
    public ButtonHandler aboutButton;
    public ButtonHandler exitButton;

    [Header("Navigation Buttons")]
    public ButtonHandler backButton;

    [Header("Title References")]
    public TMP_Text titleText;
    public TMP_Text versionText;

    private SceneLoader sceneLoader;

    /// <summary>
    /// ������������� ��� ������
    /// </summary>
    private void Start()
    {
        sceneLoader = SceneLoader.Instance;
        InitializeButtons();
        ShowMainSection();

        // ��������� ������ ����������
        if (versionText != null)
        {
            versionText.text = $"Version {Application.version}";
        }
    }

    /// <summary>
    /// ������������� ������������ ������
    /// </summary>
    private void InitializeButtons()
    {
        if (newProjectButton != null)
            newProjectButton.OnClick.AddListener(OnNewProjectClick);

        if (loadProjectButton != null)
            loadProjectButton.OnClick.AddListener(OnLoadProjectClick);

        if (settingsButton != null)
            settingsButton.OnClick.AddListener(OnSettingsClick);

        if (aboutButton != null)
            aboutButton.OnClick.AddListener(OnAboutClick);

        if (exitButton != null)
            exitButton.OnClick.AddListener(OnExitClick);

        if (backButton != null)
            backButton.OnClick.AddListener(OnBackClick);
    }

    /// <summary>
    /// ���������� �������� ������ �������
    /// </summary>
    private void OnNewProjectClick()
    {
        EventSystem.TriggerStatusMessage("Creating new project...");
        if (sceneLoader != null)
        {
            sceneLoader.LoadConstructor();
        }
    }

    /// <summary>
    /// ���������� �������� �������
    /// </summary>
    private void OnLoadProjectClick()
    {
        EventSystem.TriggerStatusMessage("Loading project...");
        // TODO: ����������� �������� �������
        EventSystem.TriggerStatusMessage("Load project functionality is coming soon!");
    }

    /// <summary>
    /// ���������� �������� ��������
    /// </summary>
    private void OnSettingsClick()
    {
        ShowSettingsSection();
        EventSystem.TriggerStatusMessage("Settings opened");
    }

    /// <summary>
    /// ���������� �������� ����������
    /// </summary>
    private void OnAboutClick()
    {
        ShowAboutSection();
        EventSystem.TriggerStatusMessage("About section opened");
    }

    /// <summary>
    /// ���������� ������ �� ����������
    /// </summary>
    private void OnExitClick()
    {
        EventSystem.TriggerStatusMessage("Exiting application...");
        Application.Quit();

        // ��� ��������� Unity
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /// <summary>
    /// ���������� ������ �����
    /// </summary>
    private void OnBackClick()
    {
        ShowMainSection();
        EventSystem.TriggerStatusMessage("Returned to main menu");
    }

    /// <summary>
    /// �������� ������� ������
    /// </summary>
    public void ShowMainSection()
    {
        HideAllSections();
        if (mainSection != null) mainSection.SetActive(true);
        if (backButton != null) backButton.gameObject.SetActive(false);
        if (titleText != null) titleText.text = "Raspberry Pi Simulator";
    }

    /// <summary>
    /// �������� ������ ��������
    /// </summary>
    public void ShowSettingsSection()
    {
        HideAllSections();
        if (settingsSection != null) settingsSection.SetActive(true);
        if (backButton != null) backButton.gameObject.SetActive(true);
        if (titleText != null) titleText.text = "Settings";
    }

    /// <summary>
    /// �������� ������ ����������
    /// </summary>
    public void ShowAboutSection()
    {
        HideAllSections();
        if (aboutSection != null) aboutSection.SetActive(true);
        if (backButton != null) backButton.gameObject.SetActive(true);
        if (titleText != null) titleText.text = "About";
    }

    /// <summary>
    /// ������ ��� ������
    /// </summary>
    private void HideAllSections()
    {
        if (mainSection != null) mainSection.SetActive(false);
        if (settingsSection != null) settingsSection.SetActive(false);
        if (aboutSection != null) aboutSection.SetActive(false);
    }

    /// <summary>
    /// ��������� interactable ���������
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (newProjectButton != null)
            newProjectButton.SetInteractable(interactable);

        if (loadProjectButton != null)
            loadProjectButton.SetInteractable(interactable);

        if (settingsButton != null)
            settingsButton.SetInteractable(interactable);

        if (aboutButton != null)
            aboutButton.SetInteractable(interactable);

        if (exitButton != null)
            exitButton.SetInteractable(interactable);

        if (backButton != null)
            backButton.SetInteractable(interactable);
    }

    /// <summary>
    /// ������� ��� �����������
    /// </summary>
    private void OnDestroy()
    {
        // ������������ �� ���� ������
        if (newProjectButton != null)
            newProjectButton.OnClick.RemoveListener(OnNewProjectClick);

        if (loadProjectButton != null)
            loadProjectButton.OnClick.RemoveListener(OnLoadProjectClick);

        if (settingsButton != null)
            settingsButton.OnClick.RemoveListener(OnSettingsClick);

        if (aboutButton != null)
            aboutButton.OnClick.RemoveListener(OnAboutClick);

        if (exitButton != null)
            exitButton.OnClick.RemoveListener(OnExitClick);

        if (backButton != null)
            backButton.OnClick.RemoveListener(OnBackClick);
    }
}