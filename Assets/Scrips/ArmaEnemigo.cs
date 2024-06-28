using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ArmaEnemigo : MonoBehaviour
{
    private Vector3 direccion;
    [SerializeField] private GameObject enemy;
    [SerializeField] private Transform pivote;
    [SerializeField] private GameObject bala;
    [SerializeField] private GameObject armaSprite;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float velocidadBala;
    private bool proyectilDisparado = false;

    void Update()
    {
        
        foreach (var hit in enemy.GetComponent<Enemigo>().hits)
        {
            if (hit.gameObject.CompareTag("Player")) 
            {
                if ((enemy.GetComponent<Enemigo>().playerDetectado && !proyectilDisparado) || enemy.GetComponent<Enemigo>().detectandoPlayer)
                {
                    direccion = hit.bounds.center - pivote.transform.position;
                    float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
                    Quaternion targetRotation = Quaternion.AngleAxis(angulo, Vector3.forward);
                    if (angulo > 90 || angulo < -90)
                    {
                        pivote.transform.localScale = new Vector3(16.31f, -16.31f, 16.31f);
                    }
                    else
                    {
                        pivote.transform.localScale = new Vector3(16.31f, 16.31f, 16.31f);

                    }
                    // Ajusta la velocidad de rotación según ea necesario
                    Quaternion newRotation = Quaternion.Lerp(pivote.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                    pivote.transform.rotation = new Quaternion(0, 0, newRotation.z, newRotation.w);
                }
                else if(enemy.GetComponent<Enemigo>().detectandoPlayer|| !enemy.GetComponent<Enemigo>().playerDetectado)
                {
                    if (enemy.transform.rotation.y==0)
                    {
                        Quaternion newRotation = Quaternion.Lerp(pivote.transform.rotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * rotationSpeed);
                        pivote.transform.rotation = new Quaternion(0, 0, newRotation.z, newRotation.w);
                    }
                    else
                    {
                        Quaternion newRotation = Quaternion.Lerp(pivote.transform.rotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * rotationSpeed);
                        pivote.transform.rotation = new Quaternion(0, 180, newRotation.z, newRotation.w);
                    }
                    // Aquí se elimina la rotación en los ejes X y Y
                                                                                                    //armaSprite.transform.localRotation = Quaternion.Euler(0, armaSprite.tra1nsform.rotation.y, armaSprite.transform.rotation.z);
                    pivote.transform.localScale = new Vector3(16.31f, 16.31f, 16.31f);

                    //pivote.transform.rotation = Quaternion.Lerp(pivote.transform.rotation, enemy.transform.rotation, Time.deltaTime * rotationSpeed);
                    ////armaSprite.transform.localRotation = Quaternion.Euler(0, armaSprite.tra1nsform.rotation.y, armaSprite.transform.rotation.z);
                    //pivote.transform.localScale = new Vector3(16.31f, 16.31f, 16.31f);
                }
            }
            
        }
    }

    private void Shoot()
    {

        var balaa = Instantiate(bala, transform.position, pivote.transform.rotation);
        balaa.GetComponent<Proyectil>().SetDireccion(direccion,velocidadBala); // Pasa la dirección al proyectil
        balaa.transform.parent = null; // Desvincula el proyectil del objeto padre
        Destroy(balaa, 2);
    }

    private void OnEnable()
    {
        Eventos.eve.disparoEnemigo.AddListener(Shoot);
    }
    private void OnDisable()
    {
        Eventos.eve.disparoEnemigo.RemoveListener(Shoot);
    }
}
