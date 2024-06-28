using UnityEngine;
using UnityEngine.UI;


public class VolumeControl : MonoBehaviour
{
    public AudioSource[] audioSources; // Array de AudioSources
    public Slider volumeSlider; // Slider para controlar el volumen

    private void Start()
    {
        // Cargar el valor del volumen guardado
        float volume = PlayerPrefs.GetFloat("volumeMusic" + volumeSlider.name,0.5f);
        SetVolume(volume);
        // Configura el valor del slider
        volumeSlider.value = volume;
        // Añade un oyente al evento onValueChanged del slider
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        // Encuentra todos los AudioSource en la escena
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource != null)
            {
                audioSource.volume = value;
            }
        }

        // Guarda la configuración del volumen
        PlayerPrefs.SetFloat("volumeMusic" + volumeSlider.name, value);
    }
}
