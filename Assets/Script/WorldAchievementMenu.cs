using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to the main "Achievement" scene's Canvas. Handles the 4 world buttons
/// (Add_Ach, Minus_Ach, Multiply_Ach, Divide_Ach): tapping one visually marks it
/// active (others become inactive/dimmed) and loads that world's dedicated
/// achievement scene, e.g. "Add_Achievement".
/// Mirrors your existing WorldSelectMenu.cs pattern, just pointed at the
/// Achievement scenes instead of the gameplay World scenes.
/// </summary>
public class WorldAchievementMenu : MonoBehaviour
{
    [System.Serializable]
    public struct WorldButtonEntry
    {
        public Button button;
        public string achievementSceneName; // e.g. "Add_Achievement"
    }

    [Header("Order must match: Add, Minus, Multiply, Divide")]
    [SerializeField] private WorldButtonEntry[] worldButtons;

    [Header("Active/inactive visual feedback")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    private void Awake()
    {
        for (int i = 0; i < worldButtons.Length; i++)
        {
            int index = i; // capture for closure
            if (worldButtons[i].button != null)
            {
                worldButtons[i].button.onClick.AddListener(() => OnWorldButtonClicked(index));
            }
        }
    }

    private void OnWorldButtonClicked(int clickedIndex)
    {
        // Mark the clicked button active, all others inactive (dimmed)
        for (int i = 0; i < worldButtons.Length; i++)
        {
            var img = worldButtons[i].button.GetComponent<Image>();
            if (img != null)
            {
                img.color = (i == clickedIndex) ? activeColor : inactiveColor;
            }
        }

        SceneManager.LoadScene(worldButtons[clickedIndex].achievementSceneName);
    }
}
