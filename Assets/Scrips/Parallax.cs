using UnityEngine;
using Cinemachine;

public class Parallax : MonoBehaviour
{
    public Transform[] backgrounds;
    private float[] parallaxScales;
    public float smoothing = 1f;
    public CinemachineVirtualCamera virtualCamera; // Agrega una referencia a la cámara virtual de Cinemachine

    private Vector3 previousCamPos;

    void Start()
    {
        previousCamPos = virtualCamera.transform.position; // Usar la posición de la cámara virtual de Cinemachine
        parallaxScales = new float[backgrounds.Length];

        for (int i = 0; i < backgrounds.Length; i++)
        {
            parallaxScales[i] = backgrounds[i].position.z * -1;
        }
    }
    private void FixedUpdate()
    {
        for (int i = 0; i < backgrounds.Length; i++)
        {
            float parallax = (previousCamPos.x - virtualCamera.transform.position.x) * parallaxScales[i];
            float backgroundTargetPosX = backgrounds[i].position.x + parallax;
            Vector3 backgroundTargetPos = new Vector3(backgroundTargetPosX, backgrounds[i].position.y, backgrounds[i].position.z);
            backgrounds[i].position = Vector3.Lerp(backgrounds[i].position, backgroundTargetPos, smoothing * Time.deltaTime);
        }

        previousCamPos = virtualCamera.transform.position; // Actualizar la posición de la cámara virtual
    }

}