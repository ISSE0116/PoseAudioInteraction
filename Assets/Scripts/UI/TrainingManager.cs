using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// トレーニング画面のUI管理クラス
/// 元動画再生・NEDO再生（CSV+音声）・インカメ映像の制御を統合
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

    [Header("NEDO Modules")]
    public NEDO02 nedo02;
    public NEDO06 nedo06;
    public NEDO46 nedo46;

    private NEDOBase activeNedo;                 // 現在アクティブなNEDOモジュール

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

        // NEDO再生を開始（CSV + AudioSource）
        StartNedoPlayback(selectedId);
    }

    void OnDisable()
    {
        // Canvas非表示時に全て停止
        if (videoController != null)
        {
            videoController.Stop();
        }

        StopNedoPlayback();
    }

    /// <summary>
    /// NEDO IDに応じたNEDOモジュールの再生を開始する
    /// </summary>
    private void StartNedoPlayback(string nedoId)
    {
        if (string.IsNullOrEmpty(nedoId))
            return;

        // 前回のコールバックをクリア
        if (activeNedo != null)
        {
            activeNedo.onPlaybackComplete = null;
        }

        // NEDO IDに応じたモジュールを選択
        activeNedo = GetNedoModule(nedoId);

        if (activeNedo != null)
        {
            // 再生完了時にリザルト画面へ遷移
            activeNedo.onPlaybackComplete = OnNedoPlaybackComplete;
            activeNedo.StartPlayback();
            Debug.Log($"TrainingManager: {nedoId} のCSV再生を開始");
        }
        else
        {
            Debug.LogWarning($"TrainingManager: {nedoId} に対応するNEDOモジュールが未設定です");
        }
    }

    /// <summary>
    /// NEDOモジュールの再生を停止する
    /// </summary>
    private void StopNedoPlayback()
    {
        if (activeNedo != null)
        {
            activeNedo.StopPlayback();
            activeNedo.onPlaybackComplete = null;
            activeNedo = null;
        }
    }

    /// <summary>
    /// NEDO IDに応じたNEDOモジュールを取得する
    /// </summary>
    private NEDOBase GetNedoModule(string nedoId)
    {
        switch (nedoId)
        {
            case "NEDO02": return nedo02;
            case "NEDO06": return nedo06;
            case "NEDO46": return nedo46;
            default: return null;
        }
    }

    /// <summary>
    /// NEDO再生完了時のコールバック
    /// </summary>
    private void OnNedoPlaybackComplete()
    {
        Debug.Log("TrainingManager: 再生完了 → リザルト画面へ遷移");
        SceneNavigator.Instance.LoadResult();
    }

    /// <summary>
    /// NEDO IDに応じて元動画ファイルを設定・再生する
    /// </summary>
    private void SetupVideoForNedo(string nedoId)
    {
        if (videoController == null || string.IsNullOrEmpty(nedoId))
            return;

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
            case "NEDO02": return "NEDO02_Original.mov";
            case "NEDO06": return "NEDO06_Original.mov";
            case "NEDO46": return "NEDO46_Original.mov";
            default: return null;
        }
    }

    private void OnBack()
    {
        SceneNavigator.Instance.LoadHome();
    }

    private void OnFinish()
    {
        SceneNavigator.Instance.LoadResult();
    }
}
