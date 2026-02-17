using System.Collections;
using System.Collections.Generic;
using Mediapipe;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample;
using UnityEngine;
using NormalizedLandmark = Mediapipe.Tasks.Components.Containers.NormalizedLandmark;

/// <summary>
/// WebCamTextureからMediapipe PoseLandmarkerを使ってリアルタイム姿勢推定を行うクラス
/// 検出されたランドマーク座標を外部に公開する
/// </summary>
public class PoseEstimator : MonoBehaviour
{
    [Header("WebCam Reference")]
    public WebCamDisplay webCamDisplay;

    [Header("Settings")]
    [Tooltip("モデルタイプ: lite, full, heavy")]
    public ModelType modelType = ModelType.Full;

    [Tooltip("最小検出信頼度")]
    [Range(0f, 1f)]
    public float minDetectionConfidence = 0.5f;

    [Tooltip("最小追跡信頼度")]
    [Range(0f, 1f)]
    public float minTrackingConfidence = 0.5f;

    [Header("Debug")]
    public bool logLandmarks = false;

    // ========== 公開プロパティ ==========

    /// <summary>
    /// 最新のランドマーク座標リスト（33点）
    /// </summary>
    public IReadOnlyList<NormalizedLandmark> LatestLandmarks { get; private set; }

    /// <summary>
    /// ランドマークが検出されているか
    /// </summary>
    public bool IsDetected => LatestLandmarks != null && LatestLandmarks.Count > 0;

    // ========== 内部変数 ==========

    private PoseLandmarker _poseLandmarker;
    private Texture2D _inputTexture;
    private bool _isRunning = false;
    private bool _resourceManagerInitialized = false;

    // ========== モデルタイプ ==========

    public enum ModelType
    {
        Lite,
        Full,
        Heavy
    }

    private string GetModelPath()
    {
        switch (modelType)
        {
            case ModelType.Lite: return "pose_landmarker_lite.bytes";
            case ModelType.Full: return "pose_landmarker_full.bytes";
            case ModelType.Heavy: return "pose_landmarker_heavy.bytes";
            default: return "pose_landmarker_full.bytes";
        }
    }

    // ========== ライフサイクル ==========

    void OnEnable()
    {
        StartCoroutine(InitializeAndRun());
    }

    void OnDisable()
    {
        StopEstimation();
    }

    void OnDestroy()
    {
        StopEstimation();
    }

    // ========== 初期化 ==========

    private IEnumerator InitializeAndRun()
    {
        // ResourceManager の初期化（AssetLoader にリソースマネージャを提供）
        if (!_resourceManagerInitialized)
        {
            AssetLoader.Provide(new StreamingAssetsResourceManager());
            _resourceManagerInitialized = true;
        }

        // モデルファイルの準備
        string modelPath = GetModelPath();
        Debug.Log($"PoseEstimator: モデル読み込み中 - {modelPath}");
        yield return AssetLoader.PrepareAssetAsync(modelPath);

        // PoseLandmarker の作成（IMAGEモードで同期的に処理）
        var options = new PoseLandmarkerOptions(
            new Mediapipe.Tasks.Core.BaseOptions(
                Mediapipe.Tasks.Core.BaseOptions.Delegate.CPU,
                modelAssetPath: modelPath
            ),
            runningMode: Mediapipe.Tasks.Vision.Core.RunningMode.IMAGE,
            numPoses: 1,
            minPoseDetectionConfidence: minDetectionConfidence,
            minPosePresenceConfidence: 0.5f,
            minTrackingConfidence: minTrackingConfidence,
            outputSegmentationMasks: false
        );

        _poseLandmarker = PoseLandmarker.CreateFromOptions(options);
        _isRunning = true;

        Debug.Log("PoseEstimator: 初期化完了、推定を開始");

        // メインループ
        var result = PoseLandmarkerResult.Alloc(1, false);
        var imageProcessingOptions = new Mediapipe.Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: 0);

        while (_isRunning)
        {
            yield return new WaitForEndOfFrame();

            if (!_isRunning || webCamDisplay == null)
                break;

            // WebCamTextureから画像を取得
            var webCamTexture = webCamDisplay.GetWebCamTexture();
            if (webCamTexture == null || !webCamTexture.isPlaying)
                continue;

            // Texture2Dに変換
            if (_inputTexture == null ||
                _inputTexture.width != webCamTexture.width ||
                _inputTexture.height != webCamTexture.height)
            {
                if (_inputTexture != null)
                    Destroy(_inputTexture);
                _inputTexture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);
            }

            _inputTexture.SetPixels32(webCamTexture.GetPixels32());
            _inputTexture.Apply();

            // Image を直接 Texture2D から作成
            using var image = new Image(_inputTexture);

            // 姿勢推定を実行
            if (_poseLandmarker.TryDetect(image, imageProcessingOptions, ref result))
            {
                if (result.poseLandmarks != null && result.poseLandmarks.Count > 0)
                {
                    LatestLandmarks = result.poseLandmarks[0].landmarks;

                    if (logLandmarks && LatestLandmarks.Count > 0)
                    {
                        // 肩の位置をログ出力（左肩=11, 右肩=12）
                        var leftShoulder = LatestLandmarks[11];
                        var rightShoulder = LatestLandmarks[12];
                        Debug.Log($"PoseEstimator: 左肩({leftShoulder.x:F2}, {leftShoulder.y:F2}) " +
                                  $"右肩({rightShoulder.x:F2}, {rightShoulder.y:F2})");
                    }
                }
                else
                {
                    LatestLandmarks = null;
                }
            }
        }
    }

    // ========== 停止 ==========

    private void StopEstimation()
    {
        _isRunning = false;

        if (_poseLandmarker != null)
        {
            _poseLandmarker.Close();
            _poseLandmarker = null;
        }

        if (_inputTexture != null)
        {
            Destroy(_inputTexture);
            _inputTexture = null;
        }

        LatestLandmarks = null;
        Debug.Log("PoseEstimator: 停止");
    }

    // ========== ランドマーク便利メソッド ==========

    /// <summary>
    /// 指定インデックスのランドマーク座標を取得する
    /// </summary>
    public Vector3 GetLandmarkPosition(int index)
    {
        if (LatestLandmarks == null || index >= LatestLandmarks.Count)
            return Vector3.zero;

        var lm = LatestLandmarks[index];
        return new Vector3(lm.x, lm.y, lm.z);
    }

    /// <summary>
    /// 左肩の座標を取得（index: 11）
    /// </summary>
    public Vector3 GetLeftShoulder() => GetLandmarkPosition(11);

    /// <summary>
    /// 右肩の座標を取得（index: 12）
    /// </summary>
    public Vector3 GetRightShoulder() => GetLandmarkPosition(12);

    /// <summary>
    /// 鼻の座標を取得（index: 0）
    /// </summary>
    public Vector3 GetNose() => GetLandmarkPosition(0);
}
