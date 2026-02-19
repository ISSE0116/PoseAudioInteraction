using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 結果画面のUI管理クラス
/// </summary>
public class ResultManager : MonoBehaviour
{
    [Header("UI Controls")]
    public Button btnHome;
    public Button btnRetry;
    public TextMeshProUGUI txtResultScore;

    void Start()
    {
        // ボタンリスナー設定（1回だけ）
        if (btnHome != null)
            btnHome.onClick.AddListener(OnHome);

        if (btnRetry != null)
            btnRetry.onClick.AddListener(OnRetry);
    }

    void OnEnable()
    {
        // Canvas表示時にスコアを表示
        if (txtResultScore != null)
        {
            float cosScore = SceneNavigator.Instance != null ? SceneNavigator.Instance.LastScore : 0f;
            float dtwScore = SceneNavigator.Instance != null ? SceneNavigator.Instance.LastDtwScore : 0f;
            txtResultScore.text = $"コサイン類似度: {cosScore:F1} 点\nDTW: {dtwScore:F1} 点";
        }
    }

    private void OnHome()
    {
        SceneNavigator.Instance.LoadHome();
    }

    private void OnRetry()
    {
        // 直前のトレーニングを再開
        string currentId = SceneNavigator.Instance.SelectedNedoId;
        SceneNavigator.Instance.LoadTraining(currentId);
    }
}
