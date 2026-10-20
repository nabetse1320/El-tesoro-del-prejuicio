using UnityEngine;
using Cinemachine;

public class Parallax : MonoBehaviour
{
    public Transform[] backgrounds;
    private float[] parallaxScales;
    [Tooltip("Intensidad del parallax (se mantiene la misma escala que antes)")]
    public float smoothing = 1f;
    public CinemachineVirtualCamera virtualCamera; // Agrega una referencia a la cámara virtual de Cinemachine

    private Transform camaraReal;
    private float previousCamX;

    void Start()
    {
        parallaxScales = new float[backgrounds.Length];

        for (int i = 0; i < backgrounds.Length; i++)
        {
            // Antes el Lerp en FixedUpdate solo aplicaba la fraccion smoothing * fixedDeltaTime
            // del desplazamiento; se conserva esa misma intensidad pero aplicada de forma exacta
            parallaxScales[i] = backgrounds[i].position.z * -1 * Mathf.Clamp01(smoothing * Time.fixedDeltaTime);
        }

        if (Camera.main != null)
        {
            camaraReal = Camera.main.transform;
            previousCamX = camaraReal.position.x;
        }
    }

    private void OnEnable()
    {
        // Se ejecuta justo despues de que Cinemachine mueve la camara en el frame,
        // asi el fondo se desplaza exactamente lo mismo que la camara y sin desfase
        CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
    }

    private void OnDisable()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
    }

    private void OnCameraUpdated(CinemachineBrain brain)
    {
        if (camaraReal == null || brain.OutputCamera == null || brain.OutputCamera.transform != camaraReal) return;

        float camX = camaraReal.position.x;
        float delta = previousCamX - camX;
        previousCamX = camX;

        // Solo se mueven los fondos de la zona cuya camara virtual esta activa
        if (virtualCamera == null || virtualCamera.Priority <= 0 || Mathf.Approximately(delta, 0f)) return;

        for (int i = 0; i < backgrounds.Length; i++)
        {
            Vector3 pos = backgrounds[i].position;
            pos.x += delta * parallaxScales[i];
            backgrounds[i].position = pos;
        }
    }

}
