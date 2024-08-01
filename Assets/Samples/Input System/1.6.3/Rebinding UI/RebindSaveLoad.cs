using UnityEngine;
using UnityEngine.InputSystem;

public class RebindSaveLoad : MonoBehaviour
{
    public InputActionAsset actions;

    private void Start()
    {
        ChargeRebinds();
    }
    //public void OnEnable()
    //{
    //    ChargeRebinds();
    //}

    //public void OnDisable()
    //{
    //    var rebinds = actions.SaveBindingOverridesAsJson();
    //    PlayerPrefs.SetString("rebinds", rebinds);
    //}
    public void ChargeRebinds()
    {
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
        Debug.Log("Se cargo");
    }
    public void SaveRebinds()
    {
        var rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
    }
}
