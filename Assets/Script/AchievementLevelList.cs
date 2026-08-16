using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this to the Canvas/controller in EACH individual achievement scene
/// (Add_Achievement, Minus_Achievement, Multiply_Achievement, Divide_Achievement).
/// Set 'worldKey' differently in each scene's Inspector (like GameManager's
/// operationType field) - the script itself is identical across all 4 scenes.
/// </summary>
public class AchievementLevelList : MonoBehaviour
{
    [Tooltip("Set per-scene: 'Add' in Add_Achievement, 'Minus' in Minus_Achievement, etc. Must match LeaderboardData's world keys exactly.")]
    [SerializeField] private string worldKey = "Add";

    [Tooltip("The parent that rows get spawned into - e.g. Add_Table.")]
    [SerializeField] private Transform rowContainer;

    [Tooltip("Drag the Add_bar row as a PROJECT PREFAB here (not a scene instance).")]
    [SerializeField] private GameObject rowPrefab;

    [SerializeField] private int totalLevels = 10;

    private void Start()
    {
        ClearExistingRows();

        for (int level = 1; level <= totalLevels; level++)
        {
            GameObject rowObj = Instantiate(rowPrefab, rowContainer);
            LevelResultRow row = rowObj.GetComponent<LevelResultRow>();

            if (row == null)
            {
                Debug.LogError("rowPrefab is missing the LevelResultRow component.");
                continue;
            }

            int stars = LeaderboardData.GetLevelStars(worldKey, level);
            int score = LeaderboardData.GetLevelScore(worldKey, level);
            row.Setup(level, stars, score);
        }
    }

    /// <summary>
    /// Destroy() is deferred until end-of-frame, which can cause a freshly
    /// Instantiate()'d child's components to briefly read as null if a Layout
    /// Group recalculates mid-frame while old children are still technically
    /// present. Using DestroyImmediate here is safe since this only runs once,
    /// in Start(), before the player can interact with anything.
    /// </summary>
    private void ClearExistingRows()
    {
        var oldChildren = new List<GameObject>();
        foreach (Transform child in rowContainer)
        {
            oldChildren.Add(child.gameObject);
        }

        foreach (var obj in oldChildren)
        {
            DestroyImmediate(obj);
        }
    }
}