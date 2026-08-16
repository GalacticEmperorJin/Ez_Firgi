using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to the "Add_bar" (or Minus_bar/Multiply_bar/Divide_bar) row PREFAB.
/// Matches your hierarchy: Add_AcCircle holds the level number text,
/// Score > Points holds the score text, and 3 star Image icons show the
/// star rating using your single-star / empty-star sprites.
/// </summary>
public class LevelResultRow : MonoBehaviour
{
    [Header("Matches your Add_bar hierarchy")]
    [SerializeField] private TMP_Text levelNumberLabel;   // the "1" text inside Add_AcCircle
    [SerializeField] private TMP_Text scoreLabel;          // "Points" text inside Score

    [Header("Star icons - add 3 Image objects under Add_bar (e.g. a 'Stars' row)")]
    [Tooltip("Exactly 3 Image objects, left to right.")]
    [SerializeField] private Image[] starIcons;
    [SerializeField] private Sprite starFilledSprite;   // your single-star icon
    [SerializeField] private Sprite starEmptySprite;    // your empty-star icon

    public void Setup(int levelNumber, int starsEarned, int score)
    {
        if (levelNumberLabel != null) levelNumberLabel.text = levelNumber.ToString();
        if (scoreLabel != null) scoreLabel.text = score.ToString();

        if (starIcons == null || starIcons.Length == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] starIcons array is empty - " +
                              "drag 3 star Image children into the Star Icons field on this prefab.");
            return;
        }

        for (int i = 0; i < starIcons.Length; i++)
        {
            if (starIcons[i] == null)
            {
                Debug.LogWarning($"[{gameObject.name}] starIcons[{i}] is unassigned - skipping.");
                continue;
            }

            bool filled = i < starsEarned;
            Sprite spriteToUse = filled ? starFilledSprite : starEmptySprite;

            if (spriteToUse == null)
            {
                Debug.LogWarning($"[{gameObject.name}] {(filled ? "starFilledSprite" : "starEmptySprite")} " +
                                  "is unassigned in the Inspector.");
                continue;
            }

            starIcons[i].sprite = spriteToUse;
        }
    }
}