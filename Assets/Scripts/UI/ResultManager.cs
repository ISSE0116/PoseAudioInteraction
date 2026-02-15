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
        // Canvas表示時に毎回実行される
        if (txtResultScore != null)
        {
            txtResultScore.text = "Score: --";
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
