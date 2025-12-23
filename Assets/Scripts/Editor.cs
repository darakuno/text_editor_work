using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.InputSystem;
using System.Collections;

public class Editor : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_FontAsset[] availableFonts;
    public TMP_Dropdown fileDropdown;

    [HideInInspector] public EditorActions editorActions;

    private UIController uiController;
    private InputHandler inputHandler;
    private FileHandler fileHandler;

    public FileHandler FileHandler => fileHandler;

    public static Editor Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;

        editorActions = new EditorActions();
        fileHandler = new FileHandler(this);

        uiController = gameObject.AddComponent<UIController>();
        uiController.Initialize(this);

        inputHandler = gameObject.AddComponent<InputHandler>();
        inputHandler.Initialize(this);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void IncreaseTextSize() => uiController?.IncreaseTextSize();
    public void DecreaseTextSize() => uiController?.DecreaseTextSize();
    public void SaveText() => fileHandler?.SaveCurrentText(inputField.text);
}