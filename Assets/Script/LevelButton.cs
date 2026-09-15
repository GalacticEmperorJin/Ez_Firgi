//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;

///// <summary>
///// Attach this to your level button PREFAB (e.g. the "1" object) - NOT to the
///// LevelButtonContainer. One prefab gets instantiated 10 times by
///// LevelSelectManager, each configured with its OWN pair of sprites
///// (e.g. clone #3 gets S-3/G-3, clone #7 gets S-7/G-7) since every level
///// needs a different number baked into its image.
///// </summary>
//public class LevelButton : MonoBehaviour
//{
//    [Header("References (already on the prefab)")]
//    [SerializeField] private Image buttonImage;
//    [SerializeField] private Button button;

//    private int _levelNumber;
//    private string _targetSceneName;

//    /// <summary>
//    /// Called once by LevelSelectManager right after instantiating this prefab.
//    /// defaultSprite/completedSprite are passed in per-level (e.g. S-3/G-3 for level 3)
//    /// rather than being fixed values baked into the prefab.
//    /// </summary>
//    public void Setup(int levelNumber, bool isCompleted, string targetSceneName,
//                       Sprite defaultSprite, Sprite completedSprite)
//    {
//        _levelNumber = levelNumber;
//        _targetSceneName = targetSceneName;

//        if (buttonImage != null)
//        {
//            buttonImage.sprite = isCompleted ? completedSprite : defaultSprite;
//        }

//        if (button != null)
//        {
//            button.onClick.RemoveAllListeners();
//            button.onClick.AddListener(OnClicked);
//        }
//        else
//        {
//            Debug.LogError($"LevelButton on '{gameObject.name}' has no Button reference assigned in the Inspector.");
//        }
//    }

//    private void OnClicked()
//    {
//        PlayerPrefs.SetInt("SelectedLevel", _levelNumber);
//        PlayerPrefs.Save();

//        SceneManager.LoadScene(_targetSceneName);
//    }
//}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to your level button PREFAB (e.g. the "1" object) - NOT to the
/// LevelButtonContainer. One prefab gets instantiated 10 times by
/// LevelSelectManager, each configured with its OWN pair of sprites
/// (e.g. clone #3 gets S-3/G-3, clone #7 gets S-7/G-7) since every level
/// needs a different number baked into its image.
/// </summary>
public class LevelButton : MonoBehaviour
{
    [Header("References (already on the prefab)")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Button button;

    [Header("Completed (gold) Y adjustments - only applied when isCompleted is true")]
    [Tooltip("Multiplies the button's Y scale ONLY when completed. 1 = same height as default. E.g. 1.2 makes it 20% taller.")]
    [SerializeField] private float completedYScale = 1.2f;

    [Tooltip("Added to the button's Y anchored position ONLY when completed - e.g. 15 nudges it upward by 15px so it visually 'pops' above the row.")]
    [SerializeField] private float completedYPositionOffset = 30f;

    private RectTransform _rect;
    private Vector3 _defaultScale;
    private Vector2 _defaultAnchoredPosition;

    private int _levelNumber;
    private string _targetSceneName;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        if (_rect != null)
        {
            // Captured once, so the default (silver) state always matches whatever
            // was already laid out on the prefab, and can be restored if a level
            // ever goes from completed -> not completed (e.g. after wiping save data).
            _defaultScale = _rect.localScale;
            _defaultAnchoredPosition = _rect.anchoredPosition;
        }
    }

    /// <summary>
    /// Called once by LevelSelectManager right after instantiating this prefab.
    /// defaultSprite/completedSprite are passed in per-level (e.g. S-3/G-3 for level 3)
    /// rather than being fixed values baked into the prefab.
    /// </summary>
    public void Setup(int levelNumber, bool isCompleted, string targetSceneName,
                       Sprite defaultSprite, Sprite completedSprite)
    {
        _levelNumber = levelNumber;
        _targetSceneName = targetSceneName;

        if (buttonImage != null)
        {
            buttonImage.sprite = isCompleted ? completedSprite : defaultSprite;
        }

        ApplyCompletedTransform(isCompleted);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }
        else
        {
            Debug.LogError($"LevelButton on '{gameObject.name}' has no Button reference assigned in the Inspector.");
        }
    }

    /// <summary>
    /// Applies the Y-only scale + position tweak for the gold/completed state,
    /// or restores the original default values for silver/not-completed.
    /// X scale and X position are never touched.
    /// </summary>
    private void ApplyCompletedTransform(bool isCompleted)
    {
        if (_rect == null) return;

        if (isCompleted)
        {
            _rect.localScale = new Vector3(_defaultScale.x, _defaultScale.y * completedYScale, _defaultScale.z);
            _rect.anchoredPosition = new Vector2(_defaultAnchoredPosition.x, _defaultAnchoredPosition.y + completedYPositionOffset);
        }
        else
        {
            _rect.localScale = _defaultScale;
            _rect.anchoredPosition = _defaultAnchoredPosition;
        }
    }

    private void OnClicked()
    {
        PlayerPrefs.SetInt("SelectedLevel", _levelNumber);
        PlayerPrefs.Save();

        SceneManager.LoadScene(_targetSceneName);
    }
}

//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;

///// <summary>
///// Attach this to your level button PREFAB (e.g. the "1" object) - NOT to the
///// LevelButtonContainer. One prefab gets instantiated 10 times by
///// LevelSelectManager, each configured with its OWN pair of sprites.
/////
///// IMPORTANT: the visual sprite must live on a CHILD object (e.g. "Icon"),
///// NOT on the root "1" object itself. The root's RectTransform is controlled
///// by the parent's Grid Layout Group, which overwrites any manual position
///// change on it every layout refresh - offsetting the root simply gets
///// silently undone. The child "Icon" is invisible to the Layout Group,
///// so its position/scale can be freely adjusted without being fought.
///// </summary>
//public class LevelButton : MonoBehaviour
//{
//    [Header("References")]
//    [Tooltip("The Image on the CHILD 'Icon' object, NOT on this root object.")]
//    [SerializeField] private Image buttonImage;
//    [SerializeField] private Button button;

//    [Header("Completed (gold) Y adjustments - applied to the Icon child, not the grid-controlled root")]
//    [Tooltip("Multiplies the Icon's Y scale ONLY when completed. 1 = same height as default.")]
//    [SerializeField] private float completedYScale = 1.2f;

//    [Tooltip("Added to the Icon's Y anchored position ONLY when completed.")]
//    [SerializeField] private float completedYPositionOffset = 30f;

//    private RectTransform _iconRect;      // the CHILD's RectTransform (safe to move)
//    private Vector3 _defaultScale;
//    private Vector2 _defaultAnchoredPosition;

//    private int _levelNumber;
//    private string _targetSceneName;

//    private void Awake()
//    {
//        if (buttonImage != null)
//        {
//            _iconRect = buttonImage.rectTransform;
//            _defaultScale = _iconRect.localScale;
//            _defaultAnchoredPosition = _iconRect.anchoredPosition;
//        }
//    }

//    public void Setup(int levelNumber, bool isCompleted, string targetSceneName,
//                       Sprite defaultSprite, Sprite completedSprite)
//    {
//        _levelNumber = levelNumber;
//        _targetSceneName = targetSceneName;

//        if (buttonImage != null)
//        {
//            buttonImage.sprite = isCompleted ? completedSprite : defaultSprite;
//        }

//        ApplyCompletedTransform(isCompleted);

//        if (button != null)
//        {
//            button.onClick.RemoveAllListeners();
//            button.onClick.AddListener(OnClicked);
//        }
//        else
//        {
//            Debug.LogError($"LevelButton on '{gameObject.name}' has no Button reference assigned in the Inspector.");
//        }
//    }

//    /// <summary>
//    /// Offsets the ICON CHILD (never the root, which the Grid Layout Group owns).
//    /// </summary>
//    private void ApplyCompletedTransform(bool isCompleted)
//    {
//        if (_iconRect == null) return;

//        if (isCompleted)
//        {
//            _iconRect.localScale = new Vector3(_defaultScale.x, _defaultScale.y * completedYScale, _defaultScale.z);
//            _iconRect.anchoredPosition = new Vector2(_defaultAnchoredPosition.x, _defaultAnchoredPosition.y + completedYPositionOffset);
//        }
//        else
//        {
//            _iconRect.localScale = _defaultScale;
//            _iconRect.anchoredPosition = _defaultAnchoredPosition;
//        }
//    }

//    private void OnClicked()
//    {
//        PlayerPrefs.SetInt("SelectedLevel", _levelNumber);
//        PlayerPrefs.Save();

//        SceneManager.LoadScene(_targetSceneName);
//    }
//}