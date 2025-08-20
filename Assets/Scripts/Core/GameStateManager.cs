using UnityEngine;

/// <summary>
/// Перечисление состояний приложения
/// </summary>
public enum GameState
{
    Initializing,   // Начальная инициализация
    MainMenu,       // Главное меню
    Constructor,    // Режим конструктора
    ExecutingCode,  // Выполнение кода
    Error           // Состояние ошибки
}

/// <summary>
/// Менеджер состояния приложения с паттерном Singleton
/// Управляет переходами между состояниями и уведомляет о изменениях
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    private GameState _currentState = GameState.Initializing;

    /// <summary>
    /// Текущее состояние приложения (только для чтения)
    /// </summary>
    public GameState CurrentState => _currentState;

    /// <summary>
    /// Вызывается при создании объекта, настраивает Singleton
    /// </summary>
    private void Awake()
    {
        // Реализация паттерна Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Сохраняем между сценами
        }
        else
        {
            Destroy(gameObject); // Уничтожаем дубликаты
        }
    }

    /// <summary>
    /// Установка нового состояния приложения
    /// </summary>
    /// <param name="newState">Новое состояние</param>
    public void SetState(GameState newState)
    {
        // Если состояние не изменилось, выходим
        if (_currentState == newState) return;

        // Сохраняем предыдущее состояние
        GameState oldState = _currentState;
        _currentState = newState;

        // Уведомляем систему событий об изменении состояния
        EventSystem.TriggerGameStateChanged(oldState, newState);
        Debug.Log($"Game state changed: {oldState} -> {newState}");
    }

    /// <summary>
    /// Проверка текущего состояния
    /// </summary>
    /// <param name="state">Состояние для проверки</param>
    /// <returns>True если текущее состояние соответствует</returns>
    public bool IsState(GameState state)
    {
        return _currentState == state;
    }

    /// <summary>
    /// Проверка текущего состояния на соответствие одному из нескольких
    /// </summary>
    /// <param name="states">Массив состояний для проверки</param>
    /// <returns>True если текущее состояние соответствует одному из указанных</returns>
    public bool IsState(params GameState[] states)
    {
        foreach (GameState state in states)
        {
            if (_currentState == state) return true;
        }
        return false;
    }
}