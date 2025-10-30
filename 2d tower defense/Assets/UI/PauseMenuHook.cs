using UnityEngine;
using UnityEngine.UI;

public class PauseMenuHook : MonoBehaviour
{
    [Header("Refs inside prefab")]
    public GameObject pauseMenuPanel;
    public Button btnResume;
    public Button btnRestart;
    public Button btnMainMenu;

    [Header("External (optional)")]
    public PauseResumeButton pauseController; // nếu để trống sẽ tự tìm

    void Awake()
    {
        if (pauseController == null)
            pauseController = FindObjectOfType<PauseResumeButton>();

        if (pauseMenuPanel && pauseController)
            pauseController.pauseMenuPanel = pauseMenuPanel;

        if (btnResume && pauseController)
        {
            btnResume.onClick.RemoveAllListeners();
            btnResume.onClick.AddListener(pauseController.OnClickResume);
        }
        if (btnRestart && pauseController)
        {
            btnRestart.onClick.RemoveAllListeners();
            btnRestart.onClick.AddListener(pauseController.OnClickRestart);
        }
        if (btnMainMenu && pauseController)
        {
            btnMainMenu.onClick.RemoveAllListeners();
            btnMainMenu.onClick.AddListener(pauseController.OnClickMainMenu);
        }
    }
}
