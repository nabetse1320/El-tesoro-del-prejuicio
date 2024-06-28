using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // Start is called before the first frame update
    public InputActionAsset actionAsset; // Asigna esto en el Inspector de Unity
    private InputActionMap actionMap;

    void Awake()
    {
        actionMap = actionAsset.FindActionMap("Player");
    }

    void Start()
    {
        // Carga la configuraci�n de entrada al inicio del juego
        if (PlayerPrefs.HasKey("Rebinds"))
        {
            string rebinds = PlayerPrefs.GetString("Rebinds");
            actionMap.LoadBindingOverridesFromJson(rebinds);
        }
    }

    public void SaveBindings()
    {
        // Guarda la configuraci�n de entrada cuando sea necesario
        string rebinds = actionMap.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("Rebinds", rebinds);
        PlayerPrefs.Save();
    }
}
