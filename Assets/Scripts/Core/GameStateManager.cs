using UnityEngine;

/// Менеджер состояния приложения, управляет переходами между состояниями
/// Реализует конечный автомат для управления потоком приложения
/// </summary>
public enum GameState
{
    Initializing,   // Начальная инициализация приложения
    MainMenu,       // Главное меню
    Constructor,    // Режим конструктора
    ExecutingCode,  // Выполнение кода
    Error           // Состояние ошибки
}

public class GameStateManager
{
    private GameState _currentState = GameState.Initializing;  // Текущее состояние приложения

    public GameState CurrentState => _currentState;  // Публичное свойство для чтения текущего состояния

    /// <summary>
    /// Установка нового состояния приложения
    /// </summary>
    /// <param name="newState">Новое состояние</param>
    public void SetState(GameState newState)
    {
        if (_currentState == newState) return;  // Если состояние не изменилось, выходим

        GameState oldState = _currentState;  // Сохраняем предыдущее состояние
        _currentState = newState;            // Устанавливаем новое состояние

        // Вызываем событие изменения состояния
        EventSystem.TriggerGameStateChanged(oldState, newState);
        Debug.Log($"Game state changed: {oldState} -> {newState}");
    }

    /// <summary>
    /// Проверка текущего состояния
    /// </summary>
    /// <param name="state">Состояние для проверки</param>
    /// <returns>True если текущее состояние соответствует проверяемому</returns>
    public bool IsState(GameState state)
    {
        return _currentState == state;
    }

    /// <summary>
    /// Проверка текущего состояния на соответствие одному из нескольких
    /// </summary>
    /// <param name="states">Массив состояний для проверки</param>
    /// <returns>True если текущее состояние соответствует одному из проверяемых</returns>
    public bool IsState(params GameState[] states)
    {
        foreach (GameState state in states)
        {
            if (_currentState == state) return true;
        }
        return false;
    }
}