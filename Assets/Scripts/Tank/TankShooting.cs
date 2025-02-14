using UnityEngine;
using UnityEngine.UI;

public class TankShooting : MonoBehaviour
{
    public int m_PlayerNumber = 1; // Para identificar a los diferentes jugadores
    public Rigidbody m_Shell; // Prefab de la bomba
    public Transform m_FireTransform; // Hijo del tanque desde donde se lanzará la bomba
    public Slider m_AimSlider; // Barra que muestra la fuerza de lanzamiento de la bomba
    public AudioSource m_ShootingAudio; // Fuente de audio para el disparo
    public AudioClip m_ChargingClip; // Clip de audio al cargar el disparo
    public AudioClip m_FireClip; // Clip de audio al disparar la bomba
    public float m_MinLaunchForce = 15f; // Fuerza mínima de disparo
    public float m_MaxLaunchForce = 30f; // Fuerza máxima de disparo
    public float m_MaxChargeTime = 0.75f; // Tiempo máximo de carga

    private string m_FireButton; // Botón de disparo
    private float m_CurrentLaunchForce; // Fuerza de la bomba al disparar
    private float m_ChargeSpeed; // Velocidad de carga basada en el tiempo máximo
    private bool m_Fired; // Indica si se ha disparado

    private void OnEnable()
    {
        // Al habilitar el tanque, reseteamos la fuerza y la UI
        m_CurrentLaunchForce = m_MinLaunchForce;
        m_AimSlider.value = m_MinLaunchForce;
    }

    private void Start()
    {
        // Definir el botón de disparo basado en el número de jugador
        m_FireButton = "Fire" + m_PlayerNumber;

        // Calcular la velocidad de carga
        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
    }

    private void Update()
    {
        // Asignar el valor mínimo al slider
        m_AimSlider.value = m_MinLaunchForce;

        // Si la carga está en el máximo y no se ha disparado, disparar automáticamente
        if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
        {
            m_CurrentLaunchForce = m_MaxLaunchForce;
            Fire();
        }
        // Si se presiona el botón de disparo
        else if (Input.GetButtonDown(m_FireButton))
        {
            m_Fired = false;
            m_CurrentLaunchForce = m_MinLaunchForce;

            // Reproducir sonido de carga
            m_ShootingAudio.clip = m_ChargingClip;
            m_ShootingAudio.Play();
        }
        // Si se mantiene presionado el botón de disparo y aún no se ha disparado
        else if (Input.GetButton(m_FireButton) && !m_Fired)
        {
            // Incrementar la fuerza de disparo y actualizar el slider
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
            m_AimSlider.value = m_CurrentLaunchForce;
        }
        // Si se suelta el botón de disparo y aún no se ha disparado
        else if (Input.GetButtonUp(m_FireButton) && !m_Fired)
        {
            Fire();
        }
    }

    private void Fire()
    {
        // Evitar múltiples disparos
        m_Fired = true;

        // Crear la bomba y obtener su Rigidbody
        Rigidbody shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation);

        // Aplicar velocidad en la dirección del disparo
        shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;

        // Reproducir sonido de disparo
        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();

        // Restablecer la fuerza de lanzamiento
        m_CurrentLaunchForce = m_MinLaunchForce;
    }
}
