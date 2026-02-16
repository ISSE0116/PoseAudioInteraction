using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// トレーニング画面のUI管理クラス
/// 元動画再生とインカメ映像の制御を統合
/// </summary>
public class TrainingManager : MonoBehaviour
{
    [Header("UI Controls")]
    public Button btnBack;
    public Button btnFinish;
    public TextMeshProUGUI txtCurrentTraining;

    [Header("Video Display")]
    public VideoController videoController;      // 元動画コントローラー
    public WebCamDisplay webCamDisplay;          // インカメ表示

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

        // 選択されたNEDO IDに応じて元動画を設定・再生
        SetupVideoForNedo(selectedId);
    }

    void OnDisable()
    {
        // Canvas非表示時に動画を停止
        if (videoController != null)
        {
            videoController.Stop();
        }
        // WebCamDisplayはOnDisableで自動停止する
    }

    /// <summary>
    /// NEDO IDに応じて元動画ファイルを設定・再生する
    /// </summary>
    private void SetupVideoForNedo(string nedoId)
    {
        if (videoController == null || string.IsNullOrEmpty(nedoId))
            return;

        // NEDO IDに対応する動画ファイル名を取得
        string videoFileName = GetVideoFileName(nedoId);
        if (!string.IsNullOrEmpty(videoFileName))
        {
            videoController.SetVideoFile(videoFileName);
            videoController.Play();
            Debug.Log($"TrainingManager: {nedoId} の元動画を再生 - {videoFileName}");
        }
        else
        {
            Debug.LogWarning($"TrainingManager: {nedoId} に対応する動画ファイルが設定されていません");
        }
    }

    /// <summary>
    /// NEDO IDから動画ファイル名を取得する
    /// </summary>
    private string GetVideoFileName(string nedoId)
    {
        switch (nedoId)
        {
            case "NEDO02":
                return "NEDO02_Original.mov";
            case "NEDO06":
                return "NEDO06_Original.mov";
            case "NEDO46":
                return "NEDO46_Original.mov";
            default:
                return null;
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
