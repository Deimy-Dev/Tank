using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 5; // Número de rondas que un jugador debe ganar para ganar el juego
    public float m_StartDelay = 3f; // Delay entre las fases de RoundStarting y RoundPlaying
    public float m_EndDelay = 3f; // Delay entre las fases de RoundPlaying y RoundEnding
    public CameraControl m_CameraControl; // Referencia al script de CameraControl
    [SerializeField] private TextMeshProUGUI m_MessageText;
    // Referencia al texto para mostrar mensajes

    public GameObject m_TankPrefab; // Referencia al Prefab del Tanque
    public TankManager[] m_Tanks; // Array de TankManagers para controlar cada tanque
    private int m_RoundNumber; // Número de ronda
    private WaitForSeconds m_StartWait; // Delay hasta que la ronda empieza
    private WaitForSeconds m_EndWait; // Delay hasta que la ronda acaba
    private TankManager m_RoundWinner; // Referencia al ganador de la ronda para anunciar quién ha ganado
    private TankManager m_GameWinner; // Referencia al ganador del juego para anunciar quién ha ganado
    private int[] m_LostAttempts; // Intentos perdidos por cada jugador
    private float m_GameStartTime; // Tiempo de inicio del juego


    private void Start()
    {
        if (m_Tanks == null || m_Tanks.Length == 0)
        {
            Debug.LogError("m_Tanks no está asignado o está vacío.");
            return;
        }

        if (m_MessageText == null)
        {
            Debug.LogError("m_MessageText no está asignado.");
            return;
        }

        if (m_CameraControl == null)
        {
            Debug.LogError("m_CameraControl no está asignado.");
            return;
        }

        m_LostAttempts = new int[m_Tanks.Length]; // Inicializa el contador de intentos perdidos
        m_GameStartTime = Time.time; // Registra el tiempo de inicio del juego
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);
        SpawnAllTanks(); // Generar tanques
        SetCameraTargets(); // Ajustar cámara
        StartCoroutine(GameLoop()); // Iniciar juego
    }

    private void SpawnAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_Instance =
            Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }

    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_Tanks.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = m_Tanks[i].m_Instance.transform;
        }
        m_CameraControl.m_Targets = targets;
    }

    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (m_GameWinner != null)
        {
            ShowGameWinner();
            yield break; // Detiene la coroutine si hay un ganador
        }

        yield return new WaitForSeconds(1f); // Espera 1 segundo antes de reiniciar el loop
        yield return StartCoroutine(GameLoop()); // Ahora la coroutine espera antes de llamarse nuevamente
    }

    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();
        m_CameraControl.SetStartPositionAndSize();
        m_RoundNumber++;
        m_MessageText.text = "ROUND " + m_RoundNumber;
        yield return m_StartWait;
    }

    private IEnumerator RoundPlaying()
    {
        EnableTankControl();
        m_MessageText.text = string.Empty;

        while (!OneTankLeft())
        {
            yield return null;
        }
    }

    private IEnumerator RoundEnding()
    {
        DisableTankControl();
        m_RoundWinner = GetRoundWinner();

        if (m_RoundWinner != null)
        {
            m_RoundWinner.m_Wins++;
            m_LostAttempts[m_RoundWinner.m_PlayerNumber - 1]++; // Incrementa intentos perdidos del jugador

            // Verifica si un jugador ha perdido 5 veces
            if (m_LostAttempts[m_RoundWinner.m_PlayerNumber - 1] >= 5)
            {
                m_GameWinner = m_RoundWinner; // El ganador es el que más rondas ha ganado
            }
        }

        m_MessageText.text = EndMessage();
        yield return m_EndWait;
    }

    private bool OneTankLeft()
    {
        int numTanksLeft = 0;
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }
        return numTanksLeft <= 1;
    }

    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }
        return null;
    }

    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }
        return null;
    }

    private string EndMessage()
    {
        string message = "EMPATE!";
        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " GANA LA RONDA!";

        message += "\n\n\n\n";
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " GANA\n";
        }

        if (m_GameWinner != null)
            message = m_GameWinner.m_ColoredPlayerText + " GANA EL JUEGO!";

        return message;
    }

    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].Reset();
        }
    }

    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].EnableControl();
        }
    }

    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].DisableControl();
        }
    }

    private void ShowGameWinner()
    {
        float gameDuration = Time.time - m_GameStartTime; // Calcula el tiempo total del juego
        string winnerMessage = m_GameWinner.m_ColoredPlayerText + " GANA EL JUEGO!\n";
        winnerMessage += "Tiempo tomado: " + gameDuration.ToString("F2") + " segundos.";
        m_MessageText.text = winnerMessage; // Muestra el mensaje del ganador
    }
}
