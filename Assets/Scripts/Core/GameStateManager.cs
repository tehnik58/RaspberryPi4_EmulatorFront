using UnityEngine;

/// �������� ��������� ����������, ��������� ���������� ����� �����������
/// ��������� �������� ������� ��� ���������� ������� ����������
/// </summary>
public enum GameState
{
    Initializing,   // ��������� ������������� ����������
    MainMenu,       // ������� ����
    Constructor,    // ����� ������������
    ExecutingCode,  // ���������� ����
    Error           // ��������� ������
}

public class GameStateManager
{
    private GameState _currentState = GameState.Initializing;  // ������� ��������� ����������

    public GameState CurrentState => _currentState;  // ��������� �������� ��� ������ �������� ���������

    /// <summary>
    /// ��������� ������ ��������� ����������
    /// </summary>
    /// <param name="newState">����� ���������</param>
    public void SetState(GameState newState)
    {
        if (_currentState == newState) return;  // ���� ��������� �� ����������, �������

        GameState oldState = _currentState;  // ��������� ���������� ���������
        _currentState = newState;            // ������������� ����� ���������

        // �������� ������� ��������� ���������
        EventSystem.TriggerGameStateChanged(oldState, newState);
        Debug.Log($"Game state changed: {oldState} -> {newState}");
    }

    /// <summary>
    /// �������� �������� ���������
    /// </summary>
    /// <param name="state">��������� ��� ��������</param>
    /// <returns>True ���� ������� ��������� ������������� ������������</returns>
    public bool IsState(GameState state)
    {
        return _currentState == state;
    }

    /// <summary>
    /// �������� �������� ��������� �� ������������ ������ �� ����������
    /// </summary>
    /// <param name="states">������ ��������� ��� ��������</param>
    /// <returns>True ���� ������� ��������� ������������� ������ �� �����������</returns>
    public bool IsState(params GameState[] states)
    {
        foreach (GameState state in states)
        {
            if (_currentState == state) return true;
        }
        return false;
    }
}