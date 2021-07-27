using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameFlowManager : MonoBehaviour
{

    private int timeCountDown;
    private string timeText;
    private int hour;
    private int minute;
    private int second;
    private int interval;
    public GameObject inGameMenu;
    public Text timer;
    public GameObject enermy;
    public Transform[] spawnPoint; 
    [Header("Parameters")]
    [Tooltip("Duration of the fade-to-black at the end of the game")]
    public float endSceneLoadDelay = 3f;
    [Tooltip("The canvas group of the fade-to-black screen")]
    public CanvasGroup endGameFadeCanvasGroup;


    [Header("Win")]
    [Tooltip("This string has to be the name of the scene you want to load when winning")]
    public string winSceneName = "WinScene";
    [Tooltip("Duration of delay before the fade-to-black, if winning")]
    public float delayBeforeFadeToBlack = 4f;
    [Tooltip("Duration of delay before the win message")]
    public float delayBeforeWinMessage = 2f;
    [Tooltip("Sound played on win")]
    public AudioClip victorySound;
    [Tooltip("Prefab for the win game message")]
    public GameObject WinGameMessagePrefab;

    [Header("Lose")]
    [Tooltip("This string has to be the name of the scene you want to load when losing")]
    public string loseSceneName = "LoseScene";


    public bool gameIsEnding { get; private set; }

    PlayerCharacterController m_Player;
    NotificationHUDManager m_NotificationHUDManager;
    ObjectiveManager m_ObjectiveManager;
    float m_TimeLoadEndGameScene;
    string m_SceneToLoad;
    void Start()
    {
        m_Player = FindObjectOfType<PlayerCharacterController>();
        DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, GameFlowManager>(m_Player, this);

        m_ObjectiveManager = FindObjectOfType<ObjectiveManager>();
		DebugUtility.HandleErrorIfNullFindObject<ObjectiveManager, GameFlowManager>(m_ObjectiveManager, this);

        AudioUtility.SetMasterVolume(1);

        timeCountDown = 86400;
        hour = 24;
        minute = 0;
        second = 0;
        timeText = "24 : 00 : 00";
        interval = 2400;
    }

    void Update()
    {   // count down the time and show the time left
        if (timeCountDown >= 0 && !inGameMenu.activeSelf)
            timeCountDown -= 2;
        hour = timeCountDown / 3600;
        minute = (timeCountDown % 3600) / 60;
        second = (timeCountDown % 3600) % 60;
        if (hour > 0 && hour < 10)
            timeText = "0" + hour + " : ";
        else timeText = hour + " : ";

        if (minute > 0 && minute < 10)
            timeText = timeText + "0" + minute + " : ";
        else if (minute == 0)
            timeText = timeText + "00" + " : ";
        else
            timeText = timeText + minute + " : ";
        if (second > 0 && second < 10)
            timeText = timeText + "0" + second;
        else if (second == 0)
            timeText = timeText + "00";
        else
            timeText = timeText + second;
        timer.text = timeText;
        if (gameIsEnding)
        {
            float timeRatio = 1 - (m_TimeLoadEndGameScene - Time.time) / endSceneLoadDelay;
            endGameFadeCanvasGroup.alpha = timeRatio;

            AudioUtility.SetMasterVolume(1 - timeRatio);

            // See if it's time to load the end scene (after the delay)
            if (Time.time >= m_TimeLoadEndGameScene)
            {
                SceneManager.LoadScene(m_SceneToLoad);
                gameIsEnding = false;
            }
        }
        else
        {  // generate enermy 
            if (timeCountDown % interval <= 2)
            {
                var generatePosition = spawnPoint[Random.Range(0, spawnPoint.Length)];
                Instantiate(enermy, generatePosition.position, Quaternion.identity);
                if (interval >= 1200)
                    interval -= 50; 
            }
            // Test if player died
            if (m_Player.isDead)
                EndGame(false);

            if (timeCountDown <= 0)
                EndGame(true);
        }
    }

    void EndGame(bool win)
    {
        // unlocks the cursor before leaving the scene, to be able to click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Remember that we need to load the appropriate end scene after a delay
        gameIsEnding = true;
        endGameFadeCanvasGroup.gameObject.SetActive(true);
        if (win)
        {
            m_SceneToLoad = winSceneName;
            m_TimeLoadEndGameScene = Time.time + endSceneLoadDelay + delayBeforeFadeToBlack;

            // play a sound on win
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = victorySound;
            audioSource.playOnAwake = false;
            audioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDVictory);
            audioSource.PlayScheduled(AudioSettings.dspTime + delayBeforeWinMessage);

            // create a game message
            var message = Instantiate(WinGameMessagePrefab).GetComponent<DisplayMessage>();
            if (message)
            {
                message.delayBeforeShowing = delayBeforeWinMessage;
                message.GetComponent<Transform>().SetAsLastSibling();
            }
        }
        else
        {
            m_SceneToLoad = loseSceneName;
            m_TimeLoadEndGameScene = Time.time + endSceneLoadDelay;
        }
    }
}
