using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aves : MonoBehaviour
{
    public float tiempoMinimo = 1f;
    public float tiempoMaximo = 5f;
    private Animator animator;
    public AnimationClip animation;
    private bool estaEjecutando = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    // Update is called once per frame
    void Update()
    {
        if (!estaEjecutando)
        {

            StartCoroutine(Ejecutar());
        }
    }
    private IEnumerator Ejecutar()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(tiempoMinimo,tiempoMaximo));
            if (!estaEjecutando)
            {
                estaEjecutando = true;

                // Aquí puedes poner el código para ejecutar tu animación.
                animator.SetBool("Volando", true);

                // Espera hasta que la animación termine
                yield return new WaitForSeconds(animation.length);

                animator.SetBool("Volando", false);
                estaEjecutando = false;
            }
        }
    }
}
