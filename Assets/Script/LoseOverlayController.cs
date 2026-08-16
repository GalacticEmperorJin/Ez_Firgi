using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to a GameObject inside your "Lose" scene (e.g. the "Canvas" or "LoseUI"
/// object). Drag your Home/Redo buttons into the fields below - no need to touch each
/// Button's own OnClick() list in the Inspector, this script wires them in code via
/// Awake(), same as PauseOverlayController.
///
/// Lose is loaded ADDITIVELY on top of the current level scene (Add/MinusQuestions/etc)
/// by GameManager.ShowLoseUI(), the same pattern already used for PausePage. That means
/// this one scene/UI is reused across all 4 worlds x 10 levels instead of duplicating a
/// "lose" panel inside every Questions scene.
///
/// Finds the active level's GameManager via GameManager.Instance (the static singleton
/// set by whichever world scene is loaded underneath), so this scene never needs its
/// own GameManager reference or a cross-scene Inspector link.
/// </summary>
public class LoseOverlayController : MonoBehaviour
{
    [Header("Buttons inside this scene")]
    [SerializeField] private Button homeButton;
    [SerializeField] private Button retryButton; // "Redo" / the "C" replay icon

    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (homeButton != null) homeButton.onClick.AddListener(GoHome);
        if (retryButton != null) retryButton.onClick.AddListener(Retry);
    }

    /// <summary>
    /// Returns to the main menu. GameManager.GoToMainMenu() loads the menu scene in
    /// Single mode, which automatically unloads every other loaded scene (this Lose
    /// scene included) - so there's no need to manually UnloadSceneAsync here like
    /// PauseOverlayController.Resume() does.
    /// </summary>
    private void GoHome()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMainMenu(mainMenuSceneName);
        }
        else
        {
            // Fallback - shouldn't normally happen since Lose is only ever loaded
            // on top of a live level scene, but keeps the button working either way.
            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
        }
    }

    /// <summary>
    /// Replays the SAME level from question 1. GameManager.RestartLevel() reloads the
    /// level scene in Single mode, which - same as above - also unloads this additive
    /// Lose scene for free, so no manual cleanup is needed.
    /// </summary>
    private void Retry()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartLevel();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }
    }
}
