using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class MessageClassifier : MonoBehaviour
{
    [System.Serializable]
    public class MessagePatterns
    {
        [Header("Patterns (Simple string contains)")]
        public string[] infoPatterns = new string[]
        {
            "LED",
            "Testing GPIO",
            "Test completed",
            "Raspberry Pi",
            "Available components",
            "Type 'exit()'",
            "GPIO mode",
            "�����������",
            "�������� ����",
            "������ �� ���������",
            "���������� ��������",
            "������� �������"
        };

        public string[] pinSetupPatterns = new string[]
        {
            "setup as",
            "�������� ���",
            "GPIO setup",
            "GPIO ��������",
            "GPIO output",
            "GPIO input"
        };

        public string[] errorPatterns = new string[]
        {
            "Error",
            "������",
            "Cannot connect",
            "ImageNotFound",
            "RuntimeError",
            "?",
            "��������� ����������",
            "�� ������",
            "��� ����������",
            "���������� ��������",
            "������ ���"
        };

        public string[] successPatterns = new string[]
        {
            "?",
            "successfully",
            "completed",
            "�������",
            "����������",
            "���������� ���������",
            "����� � ������"
        };

        public string[] warningPatterns = new string[]
        {
            "Warning",
            "��������������",
            "??",
            "�� ��������",
            "not set up",
            "�� �����",
            "��������"
        };
    }

    [SerializeField] private MessagePatterns patterns;
    [SerializeField] private bool debugMode = true; // ��������� ����� �������

    public enum MessageType
    {
        Info,
        PinSetup,
        Error,
        Success,
        Warning,
        Unknown
    }

    [System.Serializable]
    public class MessageTypeSettings
    {
        public MessageType type;
        public Color color;
        public string displayName;
    }

    [SerializeField]
    private MessageTypeSettings[] typeSettings = new MessageTypeSettings[]
    {
        new MessageTypeSettings { type = MessageType.Info, color = Color.white, displayName = "����" },
        new MessageTypeSettings { type = MessageType.PinSetup, color = new Color(0.2f, 0.6f, 1f), displayName = "��������� ����" },
        new MessageTypeSettings { type = MessageType.Error, color = Color.red, displayName = "������" },
        new MessageTypeSettings { type = MessageType.Success, color = Color.green, displayName = "�����" },
        new MessageTypeSettings { type = MessageType.Warning, color = Color.yellow, displayName = "��������������" },
        new MessageTypeSettings { type = MessageType.Unknown, color = Color.gray, displayName = "����������" }
    };

    [System.Serializable]
    public class ClassifiedMessage
    {
        public string rawMessage;
        public MessageType type;
        public DateTime timestamp;
        public Color displayColor;

        public ClassifiedMessage(string message, MessageType messageType, Color color)
        {
            rawMessage = message;
            type = messageType;
            timestamp = DateTime.Now;
            displayColor = color;
        }
    }

    public event Action<ClassifiedMessage> OnMessageClassified;

    private Dictionary<MessageType, MessageTypeSettings> _typeSettingsDict;

    private void Awake()
    {
        InitializeTypeSettings();

        if (patterns == null)
        {
            patterns = new MessagePatterns();
        }
    }

    private void InitializeTypeSettings()
    {
        _typeSettingsDict = new Dictionary<MessageType, MessageTypeSettings>();
        foreach (var setting in typeSettings)
        {
            _typeSettingsDict[setting.type] = setting;
        }
    }

    public MessageType ClassifyMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return MessageType.Unknown;

        //if (debugMode) Debug.Log($"Classifying message: '{message}'");

        // ������� �������� �����!
        if (ContainsPattern(message, patterns.errorPatterns, "Error"))
            return MessageType.Error;

        if (ContainsPattern(message, patterns.successPatterns, "Success"))
            return MessageType.Success;

        if (ContainsPattern(message, patterns.warningPatterns, "Warning"))
            return MessageType.Warning;

        if (ContainsPattern(message, patterns.pinSetupPatterns, "PinSetup"))
            return MessageType.PinSetup;

        if (ContainsPattern(message, patterns.infoPatterns, "Info"))
            return MessageType.Info;

        if (debugMode) Debug.Log($"Message '{message}' classified as Unknown");
        return MessageType.Unknown;
    }

    private bool ContainsPattern(string message, string[] patternsArray, string patternTypeForDebug = "")
    {
        foreach (string pattern in patternsArray)
        {
            if (message.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                //if (debugMode && !string.IsNullOrEmpty(patternTypeForDebug))
                //    Debug.Log($"Pattern '{pattern}' matched for {patternTypeForDebug} in message: '{message}'");
                return true;
            }
        }
        return false;
    }

    public Color GetMessageColor(MessageType messageType)
    {
        if (_typeSettingsDict.TryGetValue(messageType, out var settings))
            return settings.color;

        return Color.gray;
    }

    public string GetMessageTypeName(MessageType messageType)
    {
        if (_typeSettingsDict.TryGetValue(messageType, out var settings))
            return settings.displayName;

        return "����������";
    }

    public ClassifiedMessage ClassifyAndProcessMessage(string message)
    {
        MessageType type = ClassifyMessage(message);
        Color color = GetMessageColor(type);
        var classifiedMessage = new ClassifiedMessage(message, type, color);

        OnMessageClassified?.Invoke(classifiedMessage);
        return classifiedMessage;
    }

    // ������ ��� ���������� ���������
    public void AddPinSetupPattern(string pattern)
    {
        AddPatternToArray(ref patterns.pinSetupPatterns, pattern);
    }

    public void AddErrorPattern(string pattern)
    {
        AddPatternToArray(ref patterns.errorPatterns, pattern);
    }

    public void AddSuccessPattern(string pattern)
    {
        AddPatternToArray(ref patterns.successPatterns, pattern);
    }

    public void AddWarningPattern(string pattern)
    {
        AddPatternToArray(ref patterns.warningPatterns, pattern);
    }

    public void AddInfoPattern(string pattern)
    {
        AddPatternToArray(ref patterns.infoPatterns, pattern);
    }

    private void AddPatternToArray(ref string[] array, string pattern)
    {
        Array.Resize(ref array, array.Length + 1);
        array[array.Length - 1] = pattern;
    }

    // ��� �������
    public void LogAllPatterns()
    {
        Debug.Log("Info Patterns: " + string.Join(", ", patterns.infoPatterns));
        Debug.Log("Pin Setup Patterns: " + string.Join(", ", patterns.pinSetupPatterns));
        Debug.Log("Error Patterns: " + string.Join(", ", patterns.errorPatterns));
        Debug.Log("Success Patterns: " + string.Join(", ", patterns.successPatterns));
        Debug.Log("Warning Patterns: " + string.Join(", ", patterns.warningPatterns));
    }
}
