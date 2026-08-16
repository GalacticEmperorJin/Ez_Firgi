using UnityEngine;

/// <summary>
/// Local-device "leaderboard" data layer. Since there's no server, this tracks the
/// player's own BEST-EVER result per level (stars + score), and derives per-world
/// and combined totals from that. Ranking mixes stars + score: stars always dominate
/// (multiplied way up), score only breaks ties between similar star counts.
/// </summary>
public static class LeaderboardData
{
    private const int TotalLevelsPerWorld = 10;
    private static readonly string[] AllWorlds = { "Add", "Minus", "Multiply", "Divide" };

    // ---------------- Saving a result ----------------

    /// <summary>
    /// Call this from GameManager.EndRound() every time a level finishes.
    /// Only overwrites the saved record if this run is a NEW BEST (more stars,
    /// or same stars but higher score) - so replaying a level worse than before
    /// never erases a better previous attempt.
    /// </summary>
    public static void SubmitResult(string worldKey, int level, int stars, int score)
    {
        int previousStars = GetLevelStars(worldKey, level);
        int previousScore = GetLevelScore(worldKey, level);

        bool isNewBest = stars > previousStars || (stars == previousStars && score > previousScore);

        if (isNewBest)
        {
            PlayerPrefs.SetInt(StarsKey(worldKey, level), stars);
            PlayerPrefs.SetInt(ScoreKey(worldKey, level), score);
        }

        // Completion flag stays separate (used by LevelSelectManager for gold/silver sprite)
        if (stars > 0)
        {
            PlayerPrefs.SetInt(CompletedKey(worldKey, level), 1);
        }

        PlayerPrefs.Save();
    }

    // ---------------- Per-level reads ----------------

    public static int GetLevelStars(string worldKey, int level) =>
        PlayerPrefs.GetInt(StarsKey(worldKey, level), 0);

    public static int GetLevelScore(string worldKey, int level) =>
        PlayerPrefs.GetInt(ScoreKey(worldKey, level), 0);

    public static bool IsLevelCompleted(string worldKey, int level) =>
        PlayerPrefs.GetInt(CompletedKey(worldKey, level), 0) == 1;

    // ---------------- Per-world totals ----------------

    public static int GetWorldTotalStars(string worldKey)
    {
        int total = 0;
        for (int lvl = 1; lvl <= TotalLevelsPerWorld; lvl++)
            total += GetLevelStars(worldKey, lvl);
        return total;
    }

    public static int GetWorldTotalScore(string worldKey)
    {
        int total = 0;
        for (int lvl = 1; lvl <= TotalLevelsPerWorld; lvl++)
            total += GetLevelScore(worldKey, lvl);
        return total;
    }

    public static int GetWorldMaxStars() => TotalLevelsPerWorld * 3; // 30

    // ---------------- Combined (all 4 worlds) ----------------

    public static int GetCombinedTotalStars()
    {
        int total = 0;
        foreach (var world in AllWorlds) total += GetWorldTotalStars(world);
        return total;
    }

    public static int GetCombinedTotalScore()
    {
        int total = 0;
        foreach (var world in AllWorlds) total += GetWorldTotalScore(world);
        return total;
    }

    public static int GetCombinedMaxStars() => TotalLevelsPerWorld * 3 * AllWorlds.Length; // 120

    /// <summary>
    /// The actual "leaderboard rank value" - stars weighted far above score so two
    /// players/sessions with different star counts never tie on score alone.
    /// </summary>
    public static int GetCombinedRankValue()
    {
        return GetCombinedTotalStars() * 10000 + GetCombinedTotalScore();
    }

    /// <summary>
    /// Simple tier label based on % of all possible stars earned - gives players
    /// a sense of overall rank even with only local, single-device data.
    /// </summary>
    public static string GetRankTier()
    {
        float percent = GetCombinedMaxStars() > 0
            ? (float)GetCombinedTotalStars() / GetCombinedMaxStars()
            : 0f;

        if (percent >= 0.9f) return "Platinum";
        if (percent >= 0.7f) return "Gold";
        if (percent >= 0.4f) return "Silver";
        return "Bronze";
    }

    // ---------------- Key builders ----------------

    private static string StarsKey(string worldKey, int level) => $"{worldKey}_Level{level}_Stars";
    private static string ScoreKey(string worldKey, int level) => $"{worldKey}_Level{level}_Score";
    private static string CompletedKey(string worldKey, int level) => $"{worldKey}_Level{level}_Completed";

    public static string[] GetAllWorldKeys() => AllWorlds;
}
