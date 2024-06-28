using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundGame : MonoBehaviour
{
    [SerializeField] private int sequenceNumber;
    public List<AudioSource> sounds; // Lista de sonidos
    [HideInInspector] public List<int> sequence; // Secuencia de sonidos
    [HideInInspector] public int currentSound = 0; // Índice del sonido actual en la secuencia
    private bool isPlaying = false;

    private void Start()
    {
        GenerateSequence(sequenceNumber);
    }

    // Genera una secuencia de sonidos
    public void GenerateSequence(int length)
    {
        sequence.Clear();

        // Crea una lista de índices de sonidos
        List<int> soundIndices = new List<int>();
        for (int i = 0; i < sounds.Count; i++)
        {
            soundIndices.Add(i);
        }

        // Si la longitud de la secuencia es mayor que el número de sonidos disponibles,
        // agrega sonidos adicionales al azar
        while (soundIndices.Count < length)
        {
            soundIndices.Add(Random.Range(0, sounds.Count));
        }

        // Mezcla la lista de índices de sonidos
        for (int i = 0; i < soundIndices.Count; i++)
        {
            int temp = soundIndices[i];
            int randomIndex = Random.Range(i, soundIndices.Count);
            soundIndices[i] = soundIndices[randomIndex];
            soundIndices[randomIndex] = temp;
        }

        // Agrega los índices de sonidos a la secuencia
        sequence.AddRange(soundIndices);
    }

    // Reproduce la secuencia para el jugador
    public IEnumerator PlaySequence()
    {
        if (isPlaying)
        {
            yield break; // Si la secuencia ya está siendo reproducida, termina la corrutina
        }

        isPlaying = true; // Indica que la secuencia está siendo reproducida
        this.gameObject.GetComponent<SpriteRenderer>().color = Color.red;

        foreach (int soundIndex in sequence)
        {
            sounds[soundIndex].Play();
            yield return new WaitForSeconds(sounds[soundIndex].clip.length);
        }
        yield return new WaitForSeconds(1);
        this.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        isPlaying = false; // Indica que la secuencia ha terminado de reproducirse
    }

    // Comprueba si el sonido seleccionado por el jugador es correcto
    public void CheckSound(int soundIndex)
    {
        if (soundIndex == sequence[currentSound])
        {
            currentSound++;
            if (currentSound >= sequence.Count)
            {
                Debug.Log("Bien hecho");
                // El jugador ha completado la secuencia correctamente
                currentSound = 0;
            }
        }
        else
        {
            // El jugador ha seleccionado el sonido incorrecto
            Debug.Log("Fallaste");
            currentSound =0;
        }
    }
}
