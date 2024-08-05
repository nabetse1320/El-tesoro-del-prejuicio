using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderAll : MonoBehaviour
{
    public Slider slider1;

    void Start()
    {
        // Carga la configuraci�n del volumen
        float volume = PlayerPrefs.GetFloat("volumeAll");

        // Configura el valor del slider
        slider1.value = volume;

        // A�ade un oyente al evento onValueChanged del slider
        slider1.onValueChanged.AddListener(SetVolumeAll);
    }

    public void SetVolumeAll(float value)
    {
        // Cambia el volumen
        AudioListener.volume = value;

        // Guarda la configuraci�n del volumen
        PlayerPrefs.SetFloat("volumeAll", value);
    }
}

