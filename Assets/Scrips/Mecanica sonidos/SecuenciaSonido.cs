using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoundGame : MonoBehaviour
{
    [SerializeField] private int sequenceNumber;
    public List<AudioSource> sounds; // Lista de sonidos
    [Header("Evento a realizar")]
    [SerializeField] UnityEvent unityEvent;
    [HideInInspector] public List<int> sequence; // Secuencia de sonidos
    [HideInInspector] public int currentSound = 0; // �ndice del sonido actual en la secuencia
    private bool isPlaying = false;
    private bool puzzleIsFinish;

    #region
    public bool PuzzleIsFinish { get{ return puzzleIsFinish; } } 
    #endregion

    private void Start()
    {
        puzzleIsFinish = false;
        GenerateSequence(sequenceNumber);
    }

    // Genera una secuencia de sonidos
    public void GenerateSequence(int length)
    {
        sequence.Clear();

        // Crea una lista de �ndices de sonidos
        List<int> soundIndices = new List<int>();
        for (int i = 0; i < sounds.Count; i++)
        {
            soundIndices.Add(i);
        }

        // Si la longitud de la secuencia es mayor que el n�mero de sonidos disponibles,
        // agrega sonidos adicionales al azar
        while (soundIndices.Count < length)
        {
            soundIndices.Add(Random.Range(0, sounds.Count));
        }

        // Mezcla la lista de �ndices de sonidos
        for (int i = 0; i < soundIndices.Count; i++)
        {
            int temp = soundIndices[i];
            int randomIndex = Random.Range(i, soundIndices.Count);
            soundIndices[i] = soundIndices[randomIndex];
            soundIndices[randomIndex] = temp;
        }

        // Agrega los �ndices de sonidos a la secuencia
        sequence.AddRange(soundIndices);
    }

    // Reproduce la secuencia para el jugador
    public IEnumerator PlaySequence()
    {
        if (!puzzleIsFinish)
        {
            if (isPlaying)
            {
                yield break; // Si la secuencia ya est� siendo reproducida, termina la corrutina
            }

            isPlaying = true; // Indica que la secuencia est� siendo reproducida
            this.gameObject.GetComponent<SpriteRenderer>().color = Color.blue;

            foreach (int soundIndex in sequence)
            {
                sounds[soundIndex].Play();
                yield return new WaitForSeconds(sounds[soundIndex].clip.length);
            }
            yield return new WaitForSeconds(1);
            this.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            isPlaying = false; // Indica que la secuencia ha terminado de reproducirse
        }
        
    }

    // Comprueba si el sonido seleccionado por el jugador es correcto
    public void CheckSound(int soundIndex)
    {
        if (soundIndex == sequence[currentSound])
        {
            currentSound++;
            if (currentSound >= sequence.Count)
            {
                // El jugador ha completado la secuencia correctamente
                unityEvent.Invoke();
                currentSound = 0;
                puzzleIsFinish = true;
            }
        }
        else
        {
            // El jugador ha seleccionado el sonido incorrecto
            
            currentSound =0;
        }
    }
}
