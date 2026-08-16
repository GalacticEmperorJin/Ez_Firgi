//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;
//using TMPro;

///// <summary>
///// Drives a single level's 10-question round, matching the "Tahap 2 / 02:35 / 120 / hearts" UI shown in the mockup.
///// The timer is ONE continuous countdown for the whole level (not per-question), driven by a coroutine
///// that writes to a TMP_Text every frame using Time.deltaTime.
///// Attach to an empty GameObject in the scene and wire up the fields in the Inspector.
///// </summary>
//public class GameManager : MonoBehaviour
//{
//    [Header("Config")]
//    [SerializeField] private MathOperation operationType = MathOperation.Add;
//    [SerializeField] private int level = 1;               // "Tahap 1"
//    [SerializeField] private int questionsPerLevel = 10;
//    [SerializeField] private int startingHearts = 3;
//    [SerializeField] private int pointsPerCorrectAnswer = 10;

//    [Header("Timer - total seconds for the WHOLE level, index 0 = Level 1")]
//    [Tooltip("Level 1 gets the MOST time, Level 10 gets the LEAST - harder levels are more time-pressured.")]
//    [SerializeField]
//    private float[] levelTimeSeconds = new float[10]
//    {
//        150f, 140f, 130f, 120f, 110f, 100f, 90f, 80f, 70f, 60f
//    };

//    [Header("UI - Header")]
//    [SerializeField] private TMP_Text levelLabel;          // "Tahap 1"
//    [SerializeField] private TMP_Text timerLabel;           // "02:30"
//    [SerializeField] private TMP_Text scoreLabel;            // "120"
//    [SerializeField] private Image[] heartIcons;             // 3 heart images
//    [SerializeField] private Sprite heartFullSprite;          // red filled heart
//    [SerializeField] private Sprite heartEmptySprite;         // gray/outline heart

//    [Header("UI - Question Card")]
//    [SerializeField] private TMP_Text progressLabel;         // "7 / 10"
//    [SerializeField] private TMP_Text questionLabel;          // "456 + 378 = ?"
//    [SerializeField] private Button[] answerButtons;           // 4 buttons: A B C D
//    [SerializeField] private TMP_Text[] answerLabels;           // text inside each button

//    [Header("UI - Complete Popup (shown when all questions are answered)")]
//    [Tooltip("The 'SELESAI' popup panel - should be INACTIVE by default in the scene.")]
//    [SerializeField] private GameObject completeUI;
//    [SerializeField] private Button completeHomeButton;
//    [SerializeField] private Button completeNextButton;
//    [SerializeField] private string mainMenuSceneName = "MainMenu";
//    [SerializeField] private int totalLevelsInWorld = 10;

//    private UnitQuestionGenerator _generator;
//    private UnitQuestion _currentQuestion;
//    private int _questionIndex;      // 0-based, shown as +1
//    private int _score;
//    private int _correctCount;
//    private int _heartsRemaining;
//    private float _timeRemaining;
//    private Coroutine _timerRoutine;
//    private bool _roundEnded;
//    private bool _isPaused;

//    /// <summary>
//    /// Simple per-scene singleton (NOT persistent/DontDestroyOnLoad) so an additively-loaded
//    /// scene (like a shared PauseOverlay) can find "the current level's GameManager" at runtime,
//    /// since cross-scene Inspector references aren't possible.
//    /// </summary>
//    public static GameManager Instance { get; private set; }

//    private void Awake()
//    {
//        Instance = this;
//        _generator = new UnitQuestionGenerator();

//        if (completeUI != null) completeUI.SetActive(false);
//        if (completeHomeButton != null) completeHomeButton.onClick.AddListener(() => GoToMainMenu(mainMenuSceneName));
//        if (completeNextButton != null) completeNextButton.onClick.AddListener(GoToNextLevel);
//    }

//    private void Start()
//    {
//        // If the player tapped a specific level button on the level-select screen,
//        // it stored that number here - override the Inspector default with it.
//        if (PlayerPrefs.HasKey("SelectedLevel"))
//        {
//            level = PlayerPrefs.GetInt("SelectedLevel");
//        }

//        _score = 0;
//        _correctCount = 0;
//        _heartsRemaining = startingHearts;
//        _questionIndex = 0;
//        _roundEnded = false;

//        UpdateHearts();
//        UpdateScore();
//        levelLabel.text = $"Tahap {level}";

//        // Start the ONE level-wide countdown here, not per question
//        _timeRemaining = GetTimeForLevel(level);
//        _timerRoutine = StartCoroutine(LevelTimerTick());

//        LoadNextQuestion();
//    }

//    private float GetTimeForLevel(int lvl)
//    {
//        int idx = Mathf.Clamp(lvl - 1, 0, levelTimeSeconds.Length - 1);
//        return levelTimeSeconds[idx];
//    }

//    private void LoadNextQuestion()
//    {
//        if (_roundEnded) return;

//        if (_questionIndex >= questionsPerLevel)
//        {
//            EndRound(finishedInTime: true);
//            return;
//        }

//        _currentQuestion = _generator.Generate(operationType, level);
//        _questionIndex++;

//        progressLabel.text = $"{_questionIndex} / {questionsPerLevel}";
//        questionLabel.text = _currentQuestion.QuestionText;

//        for (int i = 0; i < answerButtons.Length; i++)
//        {
//            string optionValue = _currentQuestion.Options[i];
//            answerLabels[i].text = optionValue;

//            Button btn = answerButtons[i];
//            btn.onClick.RemoveAllListeners();
//            btn.interactable = true;
//            btn.onClick.AddListener(() => OnAnswerSelected(optionValue));
//        }
//    }

//    private void OnAnswerSelected(string selectedValue)
//    {
//        if (_roundEnded || _isPaused) return;

//        foreach (var btn in answerButtons) btn.interactable = false; // lock while resolving

//        bool isCorrect = selectedValue == _currentQuestion.CorrectAnswer;
//        if (isCorrect)
//        {
//            _correctCount++;
//            _score += pointsPerCorrectAnswer;
//            UpdateScore();
//        }
//        else
//        {
//            _heartsRemaining = Mathf.Max(0, _heartsRemaining - 1);
//            UpdateHearts();
//        }

//        // TODO: play your green-check / red-X feedback animation here
//        // e.g. FeedbackAnimator.Show(isCorrect);

//        StartCoroutine(NextQuestionAfterDelay(0.8f));
//    }

//    private IEnumerator NextQuestionAfterDelay(float delay)
//    {
//        yield return new WaitForSeconds(delay);

//        if (_roundEnded) yield break;

//        if (_heartsRemaining <= 0)
//        {
//            EndRound(finishedInTime: false); // ran out of hearts, not time - still "didn't finish cleanly"
//            yield break;
//        }

//        LoadNextQuestion();
//    }

//    /// <summary>
//    /// The single continuous countdown for the entire level.
//    /// Runs every frame via Time.deltaTime and writes straight into the TMP_Text field -
//    /// this is what makes it a real-time, live-updating timer instead of a static label.
//    /// </summary>
//    private IEnumerator LevelTimerTick()
//    {
//        while (_timeRemaining > 0f && !_roundEnded)
//        {
//            if (_isPaused)
//            {
//                // Skip the countdown entirely this frame - timer freezes exactly where it was
//                yield return null;
//                continue;
//            }

//            _timeRemaining -= Time.deltaTime;
//            int mins = Mathf.FloorToInt(Mathf.Max(0, _timeRemaining) / 60f);
//            int secs = Mathf.FloorToInt(Mathf.Max(0, _timeRemaining) % 60f);
//            timerLabel.text = $"{mins:00}:{secs:00}";
//            yield return null;
//        }

//        if (!_roundEnded)
//        {
//            // Time fully ran out before finishing all questions
//            timerLabel.text = "00:00";
//            EndRound(finishedInTime: false);
//        }
//    }

//    private void UpdateHearts()
//    {
//        for (int i = 0; i < heartIcons.Length; i++)
//        {
//            bool isFilled = i < _heartsRemaining;

//            if (heartFullSprite != null && heartEmptySprite != null)
//            {
//                // Preferred: swap to a grayed-out/empty heart sprite (keeps layout stable)
//                heartIcons[i].sprite = isFilled ? heartFullSprite : heartEmptySprite;
//                heartIcons[i].enabled = true;
//            }
//            else
//            {
//                // Fallback if no empty-heart sprite assigned: just hide it
//                heartIcons[i].enabled = isFilled;
//            }
//        }
//    }

//    private void UpdateScore() => scoreLabel.text = _score.ToString();

//    // ---------- Public controls: Home / Replay / Pause / Resume ----------

//    /// <summary>
//    /// Freezes the level: timer stops counting down, answer buttons stop responding.
//    /// Call this from your Pause button's OnClick.
//    /// </summary>
//    public void PauseGame()
//    {
//        if (_roundEnded || _isPaused) return;

//        _isPaused = true;
//        foreach (var btn in answerButtons) btn.interactable = false;
//    }

//    /// <summary>
//    /// Un-freezes the level: timer resumes from exactly where it left off,
//    /// answer buttons become clickable again. Call this from your Resume button.
//    /// </summary>
//    public void ResumeGame()
//    {
//        if (!_isPaused) return;

//        _isPaused = false;
//        if (!_roundEnded)
//        {
//            foreach (var btn in answerButtons) btn.interactable = true;
//        }
//    }

//    /// <summary>
//    /// Restarts the CURRENT level from question 1 - simplest and safest way is to
//    /// just reload the active scene, since every field (_score, _heartsRemaining,
//    /// _questionIndex, _timeRemaining, etc.) gets freshly reset in Start().
//    /// Call this from your Replay button.
//    /// </summary>
//    public void RestartLevel()
//    {
//        Time.timeScale = 1f; // safety net in case anything elsewhere paused via timeScale
//        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//    }

//    /// <summary>
//    /// Leaves the level and returns to the main menu scene.
//    /// Call this from your Home button. Make sure "MainMenu" (or whatever you pass in)
//    /// is added to File > Build Settings > Scenes In Build, and the name matches exactly.
//    /// </summary>
//    public void GoToMainMenu(string mainMenuSceneName = "MainMenu")
//    {
//        Time.timeScale = 1f;
//        SceneManager.LoadScene(mainMenuSceneName);
//    }

//    /// <summary>
//    /// Ends the round - either because all 10 questions were answered in time,
//    /// hearts ran out, or the level-wide timer hit zero.
//    /// Computes final star rating: 3 stars only possible if the player actually
//    /// finished all questions before time ran out.
//    /// </summary>
//    private void EndRound(bool finishedInTime)
//    {
//        if (_roundEnded) return;
//        _roundEnded = true;

//        if (_timerRoutine != null) StopCoroutine(_timerRoutine);
//        foreach (var btn in answerButtons) btn.interactable = false;

//        int starRating = ComputeStarRating(_correctCount, questionsPerLevel, finishedInTime);

//        Debug.Log($"Round ended. Correct: {_correctCount}/{questionsPerLevel}, " +
//                  $"Answered: {_questionIndex}/{questionsPerLevel}, " +
//                  $"FinishedInTime: {finishedInTime}, Score: {_score}, Stars: {starRating}/3");

//        // Mark this level as "completed" (unlocks the gold sprite on the level-select
//        // screen) as long as the player earned at least 1 star - tweak this condition
//        // if you want a stricter/looser passing requirement.
//        // Save stars + score for the leaderboard/achievements screen - only keeps
//        // this run if it's a NEW BEST, and also flips the completion flag used by
//        // LevelSelectManager for the gold/silver sprite.
//        LeaderboardData.SubmitResult(operationType.ToString(), level, starRating, _score);

//        if (_heartsRemaining <= 0)
//        {
//            OnGameOver();
//        }
//        else if (finishedInTime)
//        {
//            // Player genuinely answered all 10 questions (not a timeout/heart-loss ending)
//            ShowCompleteUI();
//        }
//        // TODO: if you want a separate "game over" panel for hearts=0 or timeout,
//        // show it here in an else branch instead of just OnGameOver()'s Debug.Log.
//    }

//    /// <summary>
//    /// Pops up the "SELESAI" complete panel. The Next button auto-advances to
//    /// level+1 within the SAME world (same scene, same operationType) - if this
//    /// was the last level (10), Next is disabled since there's nowhere to go.
//    /// </summary>
//    private void ShowCompleteUI()
//    {
//        if (completeUI != null) completeUI.SetActive(true);

//        bool hasNextLevel = level < totalLevelsInWorld;
//        if (completeNextButton != null) completeNextButton.gameObject.SetActive(hasNextLevel);
//    }

//    /// <summary>
//    /// Called by the Complete UI's Next button - loads level+1 in the SAME scene
//    /// (AddQuestions/MinusQuestions/etc.), reusing the same PlayerPrefs mechanism
//    /// LevelButton uses so GameManager.Start() picks up the new level correctly.
//    /// </summary>
//    private void GoToNextLevel()
//    {
//        int nextLevel = level + 1;
//        if (nextLevel > totalLevelsInWorld) return; // safety net, button should already be hidden

//        PlayerPrefs.SetInt("SelectedLevel", nextLevel);
//        PlayerPrefs.Save();

//        Time.timeScale = 1f;
//        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//    }

//    private void OnGameOver()
//    {
//        Debug.Log("Out of hearts - game over.");
//        // TODO: show game-over screen, offer retry
//    }

//    /// <summary>
//    /// Star rating based on raw correct-answer count out of the question set:
//    /// all correct (10/10) -> 3 stars, 6+ correct -> 2 stars, 4+ correct -> 1 star,
//    /// 3 or fewer correct -> 0 stars. finishedInTime is no longer used for capping,
//    /// kept in the signature so the call site doesn't need to change.
//    /// </summary>
//    private int ComputeStarRating(int correctAnswers, int totalQuestions, bool finishedInTime)
//    {
//        if (correctAnswers >= totalQuestions) return 3;   // e.g. 10/10
//        if (correctAnswers >= 6) return 2;
//        if (correctAnswers >= 4) return 1;
//        return 0;                                          // 3 or fewer correct
//    }
//}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Drives a single level's 10-question round, matching the "Tahap 2 / 02:35 / 120 / hearts" UI shown in the mockup.
/// The timer is ONE continuous countdown for the whole level (not per-question), driven by a coroutine
/// that writes to a TMP_Text every frame using Time.deltaTime.
/// Attach to an empty GameObject in the scene and wire up the fields in the Inspector.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private MathOperation operationType = MathOperation.Add;
    [SerializeField] private int level = 1;               // "Tahap 1"
    [SerializeField] private int questionsPerLevel = 10;
    [SerializeField] private int startingHearts = 3;
    [SerializeField] private int pointsPerCorrectAnswer = 10;

    [Header("Timer - total seconds for the WHOLE level, index 0 = Level 1")]
    [Tooltip("Level 1 gets the MOST time, Level 10 gets the LEAST - harder levels are more time-pressured.")]
    [SerializeField]
    private float[] levelTimeSeconds = new float[10]
    {
        150f, 140f, 130f, 120f, 110f, 100f, 90f, 80f, 70f, 60f
    };

    [Header("UI - Header")]
    [SerializeField] private TMP_Text levelLabel;          // "Tahap 1"
    [SerializeField] private TMP_Text timerLabel;           // "02:30"
    [SerializeField] private TMP_Text scoreLabel;            // "120"
    [SerializeField] private Image[] heartIcons;             // 3 heart images
    [SerializeField] private Sprite heartFullSprite;          // red filled heart
    [SerializeField] private Sprite heartEmptySprite;         // gray/outline heart

    [Header("UI - Question Card")]
    [SerializeField] private TMP_Text progressLabel;         // "7 / 10"
    [SerializeField] private TMP_Text questionLabel;          // "456 + 378 = ?"
    [SerializeField] private Button[] answerButtons;           // 4 buttons: A B C D
    [SerializeField] private TMP_Text[] answerLabels;           // text inside each button

    [Header("UI - Complete Popup (shown when all questions are answered)")]
    [Tooltip("The 'SELESAI' popup panel - should be INACTIVE by default in the scene.")]
    [SerializeField] private GameObject completeUI;
    [SerializeField] private Button completeHomeButton;
    [SerializeField] private Button completeNextButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private int totalLevelsInWorld = 10;

    [Header("Lose Overlay (shown when hearts run out OR time runs out)")]
    [Tooltip("Shared 'Lose' scene, loaded ADDITIVELY on top of this one - same pattern as PausePage.")]
    [SerializeField] private string loseOverlaySceneName = "Lose";

    private UnitQuestionGenerator _generator;
    private UnitQuestion _currentQuestion;
    private int _questionIndex;      // 0-based, shown as +1
    private int _score;
    private int _correctCount;
    private int _heartsRemaining;
    private float _timeRemaining;
    private Coroutine _timerRoutine;
    private bool _roundEnded;
    private bool _isPaused;

    /// <summary>
    /// Simple per-scene singleton (NOT persistent/DontDestroyOnLoad) so an additively-loaded
    /// scene (like a shared PauseOverlay) can find "the current level's GameManager" at runtime,
    /// since cross-scene Inspector references aren't possible.
    /// </summary>
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _generator = new UnitQuestionGenerator();

        if (completeUI != null) completeUI.SetActive(false);
        if (completeHomeButton != null) completeHomeButton.onClick.AddListener(() => GoToMainMenu(mainMenuSceneName));
        if (completeNextButton != null) completeNextButton.onClick.AddListener(GoToNextLevel);
    }

    private void Start()
    {
        // If the player tapped a specific level button on the level-select screen,
        // it stored that number here - override the Inspector default with it.
        if (PlayerPrefs.HasKey("SelectedLevel"))
        {
            level = PlayerPrefs.GetInt("SelectedLevel");
        }

        _score = 0;
        _correctCount = 0;
        _heartsRemaining = startingHearts;
        _questionIndex = 0;
        _roundEnded = false;

        UpdateHearts();
        UpdateScore();
        levelLabel.text = $"Tahap {level}";

        // Start the ONE level-wide countdown here, not per question
        _timeRemaining = GetTimeForLevel(level);
        _timerRoutine = StartCoroutine(LevelTimerTick());

        LoadNextQuestion();
    }

    private float GetTimeForLevel(int lvl)
    {
        int idx = Mathf.Clamp(lvl - 1, 0, levelTimeSeconds.Length - 1);
        return levelTimeSeconds[idx];
    }

    private void LoadNextQuestion()
    {
        if (_roundEnded) return;

        if (_questionIndex >= questionsPerLevel)
        {
            EndRound(finishedInTime: true);
            return;
        }

        _currentQuestion = _generator.Generate(operationType, level);
        _questionIndex++;

        progressLabel.text = $"{_questionIndex} / {questionsPerLevel}";
        questionLabel.text = _currentQuestion.QuestionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            string optionValue = _currentQuestion.Options[i];
            answerLabels[i].text = optionValue;

            Button btn = answerButtons[i];
            btn.onClick.RemoveAllListeners();
            btn.interactable = true;
            btn.onClick.AddListener(() => OnAnswerSelected(optionValue));
        }
    }

    private void OnAnswerSelected(string selectedValue)
    {
        if (_roundEnded || _isPaused) return;

        foreach (var btn in answerButtons) btn.interactable = false; // lock while resolving

        bool isCorrect = selectedValue == _currentQuestion.CorrectAnswer;
        if (isCorrect)
        {
            _correctCount++;
            _score += pointsPerCorrectAnswer;
            UpdateScore();
        }
        else
        {
            _heartsRemaining = Mathf.Max(0, _heartsRemaining - 1);
            UpdateHearts();
        }

        // TODO: play your green-check / red-X feedback animation here
        // e.g. FeedbackAnimator.Show(isCorrect);

        StartCoroutine(NextQuestionAfterDelay(0.8f));
    }

    private IEnumerator NextQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_roundEnded) yield break;

        if (_heartsRemaining <= 0)
        {
            EndRound(finishedInTime: false); // ran out of hearts, not time - still "didn't finish cleanly"
            yield break;
        }

        LoadNextQuestion();
    }

    /// <summary>
    /// The single continuous countdown for the entire level.
    /// Runs every frame via Time.deltaTime and writes straight into the TMP_Text field -
    /// this is what makes it a real-time, live-updating timer instead of a static label.
    /// </summary>
    private IEnumerator LevelTimerTick()
    {
        while (_timeRemaining > 0f && !_roundEnded)
        {
            if (_isPaused)
            {
                // Skip the countdown entirely this frame - timer freezes exactly where it was
                yield return null;
                continue;
            }

            _timeRemaining -= Time.deltaTime;
            int mins = Mathf.FloorToInt(Mathf.Max(0, _timeRemaining) / 60f);
            int secs = Mathf.FloorToInt(Mathf.Max(0, _timeRemaining) % 60f);
            timerLabel.text = $"{mins:00}:{secs:00}";
            yield return null;
        }

        if (!_roundEnded)
        {
            // Time fully ran out before finishing all questions
            timerLabel.text = "00:00";
            EndRound(finishedInTime: false);
        }
    }

    private void UpdateHearts()
    {
        for (int i = 0; i < heartIcons.Length; i++)
        {
            bool isFilled = i < _heartsRemaining;

            if (heartFullSprite != null && heartEmptySprite != null)
            {
                // Preferred: swap to a grayed-out/empty heart sprite (keeps layout stable)
                heartIcons[i].sprite = isFilled ? heartFullSprite : heartEmptySprite;
                heartIcons[i].enabled = true;
            }
            else
            {
                // Fallback if no empty-heart sprite assigned: just hide it
                heartIcons[i].enabled = isFilled;
            }
        }
    }

    private void UpdateScore() => scoreLabel.text = _score.ToString();

    // ---------- Public controls: Home / Replay / Pause / Resume ----------

    /// <summary>
    /// Freezes the level: timer stops counting down, answer buttons stop responding.
    /// Call this from your Pause button's OnClick.
    /// </summary>
    public void PauseGame()
    {
        if (_roundEnded || _isPaused) return;

        _isPaused = true;
        foreach (var btn in answerButtons) btn.interactable = false;
    }

    /// <summary>
    /// Un-freezes the level: timer resumes from exactly where it left off,
    /// answer buttons become clickable again. Call this from your Resume button.
    /// </summary>
    public void ResumeGame()
    {
        if (!_isPaused) return;

        _isPaused = false;
        if (!_roundEnded)
        {
            foreach (var btn in answerButtons) btn.interactable = true;
        }
    }

    /// <summary>
    /// Restarts the CURRENT level from question 1 - simplest and safest way is to
    /// just reload the active scene, since every field (_score, _heartsRemaining,
    /// _questionIndex, _timeRemaining, etc.) gets freshly reset in Start().
    /// Call this from your Replay button.
    /// </summary>
    public void RestartLevel()
    {
        Time.timeScale = 1f; // safety net in case anything elsewhere paused via timeScale
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Leaves the level and returns to the main menu scene.
    /// Call this from your Home button. Make sure "MainMenu" (or whatever you pass in)
    /// is added to File > Build Settings > Scenes In Build, and the name matches exactly.
    /// </summary>
    public void GoToMainMenu(string mainMenuSceneName = "MainMenu")
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Ends the round - either because all 10 questions were answered in time,
    /// hearts ran out, or the level-wide timer hit zero.
    /// Computes final star rating: 3 stars only possible if the player actually
    /// finished all questions before time ran out.
    /// </summary>
    private void EndRound(bool finishedInTime)
    {
        if (_roundEnded) return;
        _roundEnded = true;

        if (_timerRoutine != null) StopCoroutine(_timerRoutine);
        foreach (var btn in answerButtons) btn.interactable = false;

        int starRating = ComputeStarRating(_correctCount, questionsPerLevel, finishedInTime);

        Debug.Log($"Round ended. Correct: {_correctCount}/{questionsPerLevel}, " +
                  $"Answered: {_questionIndex}/{questionsPerLevel}, " +
                  $"FinishedInTime: {finishedInTime}, Score: {_score}, Stars: {starRating}/3");

        // Mark this level as "completed" (unlocks the gold sprite on the level-select
        // screen) as long as the player earned at least 1 star - tweak this condition
        // if you want a stricter/looser passing requirement.
        // Save stars + score for the leaderboard/achievements screen - only keeps
        // this run if it's a NEW BEST, and also flips the completion flag used by
        // LevelSelectManager for the gold/silver sprite.
        LeaderboardData.SubmitResult(operationType.ToString(), level, starRating, _score);

        if (finishedInTime)
        {
            // Player genuinely answered all 10 questions before hearts or time ran out
            ShowCompleteUI();
        }
        else
        {
            // Didn't finish cleanly - either hearts hit 0, OR the level-wide timer hit
            // 0 while hearts were still remaining. Both are a loss, so both show the
            // same shared Lose overlay (previously only the hearts=0 case was handled
            // here, so a timeout with hearts left fell through and showed nothing).
            ShowLoseUI();
        }
    }

    /// <summary>
    /// Pops up the "SELESAI" complete panel. The Next button auto-advances to
    /// level+1 within the SAME world (same scene, same operationType) - if this
    /// was the last level (10), Next is disabled since there's nowhere to go.
    /// </summary>
    private void ShowCompleteUI()
    {
        if (completeUI != null) completeUI.SetActive(true);

        bool hasNextLevel = level < totalLevelsInWorld;
        if (completeNextButton != null) completeNextButton.gameObject.SetActive(hasNextLevel);
    }

    /// <summary>
    /// Called by the Complete UI's Next button - loads level+1 in the SAME scene
    /// (AddQuestions/MinusQuestions/etc.), reusing the same PlayerPrefs mechanism
    /// LevelButton uses so GameManager.Start() picks up the new level correctly.
    /// </summary>
    private void GoToNextLevel()
    {
        int nextLevel = level + 1;
        if (nextLevel > totalLevelsInWorld) return; // safety net, button should already be hidden

        PlayerPrefs.SetInt("SelectedLevel", nextLevel);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Pops up the shared "Lose" scene on top of this one (additive, same pattern as
    /// PausePage) - covers BOTH ways a round can end badly: hearts hit 0, or the
    /// level-wide timer hit 0 (even if hearts were still remaining).
    /// LoseOverlayController (in the Lose scene) reads GameManager.Instance to wire
    /// its Home/Retry buttons, so nothing else needs to be passed in here.
    /// </summary>
    private void ShowLoseUI()
    {
        Time.timeScale = 1f; // safety net, matches RestartLevel/GoToMainMenu

        if (!SceneManager.GetSceneByName(loseOverlaySceneName).isLoaded)
        {
            SceneManager.LoadScene(loseOverlaySceneName, LoadSceneMode.Additive);
        }
    }

    /// <summary>
    /// Star rating based on raw correct-answer count out of the question set:
    /// all correct (10/10) -> 3 stars, 6+ correct -> 2 stars, 4+ correct -> 1 star,
    /// 3 or fewer correct -> 0 stars. finishedInTime is no longer used for capping,
    /// kept in the signature so the call site doesn't need to change.
    /// </summary>
    private int ComputeStarRating(int correctAnswers, int totalQuestions, bool finishedInTime)
    {
        if (correctAnswers >= totalQuestions) return 3;   // e.g. 10/10
        if (correctAnswers >= 6) return 2;
        if (correctAnswers >= 4) return 1;
        return 0;                                          // 3 or fewer correct
    }
}