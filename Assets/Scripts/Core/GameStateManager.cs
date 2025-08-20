using UnityEngine;

/// <summary>
/// ������������ ��������� ����������
/// </summary>
public enum GameState
{
    Initializing,   // ��������� �������������
    MainMenu,       // ������� ����
    Constructor,    // ����� ������������
    ExecutingCode,  // ���������� ����
    Error           // ��������� ������
}

/// <summary>
/// �������� ��������� ���������� � ��������� Singleton
/// ��������� ���������� ����� ����������� � ���������� � ����������
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    private GameState _currentState = GameState.Initializing;

    /// <summary>
    /// ������� ��������� ���������� (������ ��� ������)
    /// </summary>
    public GameState CurrentState => _currentState;

    /// <summary>
    /// ���������� ��� �������� �������, ����������� Singleton
    /// </summary>
    private void Awake()
    {
        // ���������� �������� Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ��������� ����� �������
        }
        else
        {
            Destroy(gameObject); // ���������� ���������
        }
    }

    /// <summary>
    /// ��������� ������ ��������� ����������
    /// </summary>
    /// <param name="newState">����� ���������</param>
    public void SetState(GameState newState)
    {
        // ���� ��������� �� ����������, �������
        if (_currentState == newState) return;

        // ��������� ���������� ���������
        GameState oldState = _currentState;
        _currentState = newState;

        // ���������� ������� ������� �� ��������� ���������
        EventSystem.TriggerGameStateChanged(oldState, newState);
        Debug.Log($"Game state changed: {oldState} -> {newState}");
    }

    /// <summary>
    /// �������� �������� ���������
    /// </summary>
    /// <param name="state">��������� ��� ��������</param>
    /// <returns>True ���� ������� ��������� �������������</returns>
    public bool IsState(GameState state)
    {
        return _currentState == state;
    }

    /// <summary>
    /// �������� �������� ��������� �� ������������ ������ �� ����������
    /// </summary>
    /// <param name="states">������ ��������� ��� ��������</param>
    /// <returns>True ���� ������� ��������� ������������� ������ �� ���������</returns>
    public bool IsState(params GameState[] states)
    {
        foreach (GameState state in states)
        {
            if (_currentState == state) return true;
        }
        return false;
    }
}