using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("References")]
    public EnemySpawner spawner;      // quản lý wave
    public LevelManager levelManager; // quản lý máu base

    [Header("UI Texts")]
    public TMP_Text txtLives;
    public TMP_Text txtWave;

    [Header("Buttons")]
    public Button btnStart;
    public Button btnNextWave;
    public Button btnPause;
    public Button btnSpeed1;
    public Button btnSpeed2;
    public Button btnSpeed3;

    private bool paused = false;

    void Start()
    {
        // Gán tham chiếu tự động nếu quên
        if (!spawner) spawner = FindObjectOfType<EnemySpawner>();
        if (!levelManager) levelManager = FindObjectOfType<LevelManager>();

        // Nút bấm
        btnStart.onClick.AddListener(OnStartClicked);
        btnNextWave.onClick.AddListener(OnNextWaveClicked);
        btnPause.onClick.AddListener(OnPauseClicked);
        btnSpeed1.onClick.AddListener(() => SetSpeed(1f));
        btnSpeed2.onClick.AddListener(() => SetSpeed(2f));
        btnSpeed3.onClick.AddListener(() => SetSpeed(3f));

        // Sự kiện wave
        spawner.onWaveChanged += OnWaveChanged;
        spawner.onAllWavesFinished += OnAllWavesFinished;

        // Sự kiện máu base
        levelManager.onLivesChanged += OnLivesChanged;

        // Khởi tạo
        btnNextWave.interactable = false;
        txtLives.text = $"❤️ {levelManager.lives}";
        txtWave.text = $"🌊 Wave 0/{spawner.TotalWaves}";
    }

    void Update()
    {
        // Cho phép NextWave khi đủ điều kiện
        btnNextWave.interactable = spawner.CanStartNextWave;
    }

    // ========== Sự kiện từ Spawner ==========
    void OnWaveChanged(int current, int total)
    {
        txtWave.text = $"🌊 Wave {current}/{total}";
        btnNextWave.interactable = false;
    }

    void OnAllWavesFinished()
    {
        txtWave.text = "✅ All waves finished!";
        btnNextWave.interactable = false;
    }

    // ========== Sự kiện từ LevelManager ==========
    void OnLivesChanged(int newLives)
    {
        txtLives.text = $"❤️ {newLives}";
    }

    // ========== Nút bấm ==========
    void OnStartClicked()
    {
        btnStart.gameObject.SetActive(false);
        spawner.StartWavesByButton();
    }

    void OnNextWaveClicked()
    {
        spawner.StartNextWaveNow();
        btnNextWave.interactable = false;
    }

    void OnPauseClicked()
    {
        paused = !paused;
        Time.timeScale = paused ? 0f : 1f;
        btnPause.GetComponentInChildren<TMP_Text>().text = paused ? "Resume" : "Pause";
    }

    void SetSpeed(float s)
    {
        if (!paused) Time.timeScale = s;
    }
}
