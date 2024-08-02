using UnityEngine;
using UnityEngine.InputSystem;

public class RebindSaveLoad : MonoBehaviour
{
    public InputActionAsset actions;

    private void Start()
    {
        //actions.Disable();
        ChargeRebinds();
        actions.Enable();
    }
    public void OnEnable()
    {
        //ChargeRebinds();
    }

    //public void OnDisable()
    //{
    //    var rebinds = actions.SaveBindingOverridesAsJson();
    //    PlayerPrefs.SetString("rebinds", rebinds);
    //}
    public void ChargeRebinds()
    {
        
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            //Debug.Log("A");
            actions.LoadBindingOverridesFromJson(rebinds);
        
    }
    public void SaveRebinds()
    {
        var rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
    }
}
