using System;
using UnityEngine;

[Serializable] // Hace que los atributos aparezcan en el inspector (si no los ocultamos)
public class TankManager
{
    // Esta clase gestiona la configuración del tanque junto con el GameManager.
    // Controla el comportamiento de los tanques y si los jugadores pueden manejarlos en distintas fases del juego.

    public Color m_PlayerColor; // Color del tanque
    public Transform m_SpawnPoint; // Posición y dirección donde se generará el tanque

    [HideInInspector] public int m_PlayerNumber; // Identificador del jugador
    [HideInInspector] public string m_ColoredPlayerText; // Texto con el color del tanque
    [HideInInspector] public GameObject m_Instance; // Referencia a la instancia del tanque creada
    [HideInInspector] public int m_Wins; // Número de victorias del jugador

    private TankMovement m_Movement; // Referencia al script de movimiento del tanque
    private TankShooting m_Shooting; // Referencia al script de disparo del tanque
    private GameObject m_CanvasGameObject; // UI del tanque que se desactiva en ciertas fases del juego

    public void Setup()
    {
        // Obtengo referencias a los componentes
        m_Movement = m_Instance.GetComponent<TankMovement>();
        m_Shooting = m_Instance.GetComponent<TankShooting>();
        m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject;

        // Asigno el número de jugador a los scripts del tanque
        m_Movement.m_PlayerNumber = m_PlayerNumber;
        m_Shooting.m_PlayerNumber = m_PlayerNumber;

        // Creo un string con el color del tanque para mostrar "PLAYER X"
        m_ColoredPlayerText = $"<color=#{ColorUtility.ToHtmlStringRGB(m_PlayerColor)}>PLAYER {m_PlayerNumber}</color>";

        // Obtengo todos los renderers del tanque y cambio su color
        MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            renderer.material.color = m_PlayerColor;
        }
    }

    // Desactiva el control del tanque en fases donde el jugador no debe controlarlo
    public void DisableControl()
    {
        m_Movement.enabled = false;
        m_Shooting.enabled = false;
        m_CanvasGameObject.SetActive(false);
    }

    // Activa el control del tanque en fases donde el jugador puede controlarlo
    public void EnableControl()
    {
        m_Movement.enabled = true;
        m_Shooting.enabled = true;
        m_CanvasGameObject.SetActive(true);
    }

    // Restablece el tanque a su estado inicial al comienzo de cada ronda
    public void Reset()
    {
        m_Instance.transform.position = m_SpawnPoint.position;
        m_Instance.transform.rotation = m_SpawnPoint.rotation;
        m_Instance.SetActive(false);
        m_Instance.SetActive(true);
    }
}
