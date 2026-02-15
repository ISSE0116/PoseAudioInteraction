using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// トレーニング画面のUI管理クラス
/// </summary>
public class TrainingManager : MonoBehaviour
{
    [Header("UI Controls")]
    public Button btnBack;
    public Button btnFinish;
    public TextMeshProUGUI txtCurrentTraining;

    void Start()
    {
        // ボタンリスナー設定（1回だけ）
        if (btnBack != null)
            btnBack.onClick.AddListener(OnBack);

        if (btnFinish != null)
            btnFinish.onClick.AddListener(OnFinish);
    }

    void OnEnable()
    {
        // Canvas表示時に毎回実行される
        string selectedId = SceneNavigator.Instance?.SelectedNedoId;
        if (txtCurrentTraining != null && !string.IsNullOrEmpty(selectedId))
        {
            txtCurrentTraining.text = $"Training: {selectedId}";
        }
    }

    private void OnBack()
    {
        SceneNavigator.Instance.LoadHome();
    }

    private void OnFinish()
    {
        // ここでスコア計算などを保存する処理が入る予定
        SceneNavigator.Instance.LoadResult();
    }
}
