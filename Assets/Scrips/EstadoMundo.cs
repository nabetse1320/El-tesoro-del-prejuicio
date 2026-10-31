using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Estado compartido por las escenas de una misma zona (Nivel 2 - 1 -> Nivel 2 - 2 -> Nivel 2 - 3).
// Solo guarda claves (strings): enemigos derrotados y mecanismos completados.
//
// - Lo que pasa en la zona de inicio queda "pendiente" y solo se confirma al entrar en la
//   escena intermedia. Asi, morir y reintentar la escena de inicio se comporta como antes.
// - Se guarda solo en memoria: al cerrar el juego se pierde.
// - Se borra al volver al menu o al entrar en cualquier escena fuera de la zona.
public static class EstadoMundo
{
    public const string EscenaInicioZona = "Nivel 2 - 1";
    public const string EscenaIntermediaZona = "Nivel 2 - 2";
    public const string EscenaFinalZona = "Nivel 2 - 3";

    // Escena de carga que usa cambiarNivel / Loader entre dos niveles. No cambia nada.
    private const string EscenaTransicion = "LoadScene";

    private const string PrefijoEnemigo = "enemigo:";
    private const string PrefijoMecanismo = "mecanismo:";

    private static readonly HashSet<string> confirmados = new HashSet<string>();
    private static readonly HashSet<string> pendientes = new HashSet<string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Inicializar()
    {
        confirmados.Clear();
        pendientes.Clear();
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        switch (escena.name)
        {
            case EscenaTransicion:
                break;

            case EscenaInicioZona:
                // Entrada normal o reintento tras morir: lo no confirmado se descarta
                pendientes.Clear();
                break;

            case EscenaIntermediaZona:
                // Se pasa de la zona de inicio a la intermedia: se confirma lo hecho
                confirmados.UnionWith(pendientes);
                pendientes.Clear();
                break;

            case EscenaFinalZona:
                // Lo hecho en la zona final no se confirma; solo se aplica lo confirmado
                pendientes.Clear();
                break;

            default:
                // Menu, nuevo juego o cualquier otra zona: se borra todo
                confirmados.Clear();
                pendientes.Clear();
                break;
        }
    }

    // ----- Enemigos -----

    public static bool EnemigoDerrotado(string id)
    {
        return Contiene(confirmados, PrefijoEnemigo, id);
    }

    public static void RegistrarEnemigoDerrotado(string id)
    {
        Registrar(PrefijoEnemigo, id);
    }

    // ----- Mecanismos (puzzles) -----

    public static bool MecanismoCompletado(string id)
    {
        return Contiene(confirmados, PrefijoMecanismo, id);
    }

    public static void RegistrarMecanismoCompletado(string id)
    {
        Registrar(PrefijoMecanismo, id);
    }

    private static bool Contiene(HashSet<string> conjunto, string prefijo, string id)
    {
        return !string.IsNullOrEmpty(id) && conjunto.Contains(prefijo + id);
    }

    private static void Registrar(string prefijo, string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            pendientes.Add(prefijo + id);
        }
    }
}
