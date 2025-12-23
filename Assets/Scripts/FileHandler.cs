using System.IO;
using System.Collections.Generic;
using Application = UnityEngine.Application;
using Debug = UnityEngine.Debug;


public class FileHandler
{
    public event System.Action<string> OnFilePathChanged;
    private string _saveFilePath;
    public string FilePath => _saveFilePath;
    private const string NoFilePath = "";
    private readonly Editor _editor;

    public FileHandler(Editor editor)
    {
        _editor = editor;
        _saveFilePath = NoFilePath;
        OnFilePathChanged?.Invoke(GetFileNameForTitle(_saveFilePath));
    }

    private string GetFileName(string path)
    {
        return Path.GetFileName(path);
    }

    private string GetFileNameForTitle(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return "Без имени*";
        }
        return Path.GetFileName(path);
    }

    public void SaveCurrentText(string content)
    {
        if (string.IsNullOrEmpty(_saveFilePath) || _saveFilePath == NoFilePath || !File.Exists(_saveFilePath))
        {
            Debug.Log("Нет пути для сохранения. Вызов 'Сохранить как...'.");
            SaveFileAs();
            return;
        }
        try
        {
            File.WriteAllText(_saveFilePath, content);
            Debug.Log("Текст успешно сохранен: " + _saveFilePath);
            OnFilePathChanged?.Invoke(GetFileNameForTitle(_saveFilePath));
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка при сохранении файла: " + e.Message);
        }
    }

    public void SaveFileAs()
    {
        string initialDir = Path.GetDirectoryName(Application.persistentDataPath);
        string currentFileName = string.IsNullOrEmpty(_saveFilePath) ? "Новый документ.txt" : GetFileName(_saveFilePath);

        string newPath = UnityEditor.EditorUtility.SaveFilePanel(
            "Сохранить как...",
            initialDir,
            currentFileName,
            "txt"
        );

        if (!string.IsNullOrEmpty(newPath))
        {
            _saveFilePath = newPath;
            string content = _editor.inputField.text;
            try
            {
                File.WriteAllText(_saveFilePath, content);
                Debug.Log("Файл успешно сохранен как: " + _saveFilePath);
                OnFilePathChanged?.Invoke(GetFileNameForTitle(_saveFilePath));
            }
            catch (System.Exception e)
            {
                Debug.LogError("Ошибка при сохранении файла: " + e.Message);
            }
        }
        else
        {
            Debug.Log("Сохранение отменено пользователем.");
        }
    }

    public void LoadFile()
    {
        string initialDir = Path.GetDirectoryName(Application.persistentDataPath); 
        string newPath = UnityEditor.EditorUtility.OpenFilePanel("Открыть файл", initialDir, "txt");

        if (!string.IsNullOrEmpty(newPath))
        {
            _saveFilePath = newPath;
            LoadContentAndNotify();
        }
    }

    private void LoadContentAndNotify()
    {
        if (File.Exists(_saveFilePath))
        {
            try
            {
                string loadedContent = File.ReadAllText(_saveFilePath);

                if (_editor != null && _editor.inputField != null)
                {
                    _editor.inputField.text = loadedContent;
                    Debug.Log("Текст успешно загружен!");
                    OnFilePathChanged?.Invoke(GetFileNameForTitle(_saveFilePath));
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Ошибка при загрузке файла: " + e.Message);
            }
        }
        else
        {
            Debug.Log("Файл не найден. Поле пусто.");
            if (_editor.inputField != null)
            {
                _editor.inputField.text = "";
            }
            OnFilePathChanged?.Invoke(GetFileNameForTitle(_saveFilePath));
        }
    }
}
