using UnityEngine;
using TMPro;

public class TrainManager : MonoBehaviour
{
    public static TrainManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI statsText;

    // 정적 정보
    private string behaviorName = "N/A";
    private string deviceInfo;
    private int maxStep;

    // 누적 통계 (모든 에이전트 합산)
    private int totalEpisodes;
    private int activeAgents;
    private int totalSteps;
    private float cumulativeReward;

    // 최근 에피소드 통계 (이동 평균)
    private float avgReward;
    private float avgSteps;
    private const int SmoothingWindow = 100;

    // 성공/실패
    private int successCount;
    private int failCount;

    // 경과 시간
    private float elapsed;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        deviceInfo = SystemInfo.supportsComputeShaders ? "GPU" : "CPU";
    }

    public void RegisterAgent(string behaviorName, int maxStep)
    {
        this.behaviorName = behaviorName;
        this.maxStep = maxStep;
        activeAgents++;
    }

    public void UnregisterAgent()
    {
        activeAgents = Mathf.Max(0, activeAgents - 1);
    }

    /// <summary>
    /// 에이전트가 매 스텝마다 호출. 보상 누적.
    /// </summary>
    public void AddStep(float reward)
    {
        totalSteps++;
        cumulativeReward += reward;
    }

    /// <summary>
    /// 에이전트의 에피소드가 끝날 때 호출.
    /// </summary>
    public void OnEpisodeEnd(float episodeReward, int episodeSteps, bool success)
    {
        totalEpisodes++;

        // 이동 평균 (exponential moving average)
        float alpha = Mathf.Min(1f, SmoothingWindow > 0 ? 2f / (SmoothingWindow + 1) : 1f);
        avgReward = totalEpisodes == 1
            ? episodeReward
            : avgReward + alpha * (episodeReward - avgReward);
        avgSteps = totalEpisodes == 1
            ? episodeSteps
            : avgSteps + alpha * (episodeSteps - avgSteps);

        if (success) successCount++;
        else failCount++;
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        if (statsText == null) return;

        float successRate = (successCount + failCount) > 0
            ? (float)successCount / (successCount + failCount) * 100f
            : 0f;

        statsText.text =
            $"[{behaviorName}]  Device: {deviceInfo}\n" +
            $"Agents: {activeAgents}  |  Time: {FormatTime(elapsed)}\n" +
            $"――――――――――――――――――\n" +
            $"Episodes: {totalEpisodes}\n" +
            $"Total Steps: {totalSteps}\n" +
            $"――――――――――――――――――\n" +
            $"Avg Reward: {avgReward:F3}\n" +
            $"Avg Steps: {avgSteps:F1}\n" +
            $"Cumulative Reward: {cumulativeReward:F1}\n" +
            $"――――――――――――――――――\n" +
            $"Success Rate: {successRate:F1}%  ({successCount}/{successCount + failCount})";
    }

    private string FormatTime(float seconds)
    {
        int h = (int)(seconds / 3600);
        int m = (int)((seconds % 3600) / 60);
        int s = (int)(seconds % 60);
        return h > 0 ? $"{h}h {m:D2}m {s:D2}s" : $"{m}m {s:D2}s";
    }
}
