using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using NormalizedLandmark = Mediapipe.Tasks.Components.Containers.NormalizedLandmark;

/// <summary>
/// 元動画の姿勢データ（CSV）とリアルタイム姿勢推定の比較による動き評価クラス
/// PoseEstimatorのランドマークからNEDO別の特徴量を計算し、CSVデータとコサイン類似度で比較する
/// </summary>
public class MovementEvaluator : MonoBehaviour
{
    [Header("References")]
    public PoseEstimator poseEstimator;

    [Header("Debug")]
    public bool logFrameScore = false;

    // ========== 内部変数 ==========

    private string _activeNedoId;
    private float _frameRate;
    private float _elapsedTime;
    private bool _isEvaluating = false;
    private int _totalCsvFrames = 0;

    // CSVデータ（NEDO別に保持）
    private readonly List<float[]> _csvFeatures = new();

    // フレームごとのスコア蓄積（コサイン類似度）
    private readonly List<float> _frameScores = new();

    // リアルタイム特徴量の蓄積（DTW用）
    private readonly List<float[]> _liveFeatures = new();

    // ========== 公開プロパティ ==========

    /// <summary>
    /// コサイン類似度ベースの最終スコア（0〜100）
    /// </summary>
    public float FinalScore { get; private set; } = 0f;

    /// <summary>
    /// DTWベースの最終スコア（0〜100）
    /// </summary>
    public float DtwScore { get; private set; } = 0f;

    /// <summary>
    /// 評価中かどうか
    /// </summary>
    public bool IsEvaluating => _isEvaluating;

    // ========== 公開メソッド ==========

    /// <summary>
    /// 評価を開始する
    /// </summary>
    /// <param name="nedoId">NEDO ID（"NEDO02", "NEDO06", "NEDO46"）</param>
    /// <param name="frameRate">CSVのフレームレート</param>
    public void StartEvaluation(string nedoId, float frameRate)
    {
        _activeNedoId = nedoId;
        _frameRate = frameRate;
        _elapsedTime = 0f;
        _frameScores.Clear();
        _liveFeatures.Clear();
        FinalScore = 0f;
        DtwScore = 0f;

        // コルーチンでCSV読み込み（Android対応）
        StartCoroutine(LoadCsvAndStartEvaluation(nedoId));
    }

    private IEnumerator LoadCsvAndStartEvaluation(string nedoId)
    {
        string csvFileName = GetCsvFileName(nedoId);
        string csvText = null;

        yield return NEDOBase.LoadStreamingAssetText(csvFileName, text => csvText = text);

        if (string.IsNullOrEmpty(csvText))
        {
            Debug.LogWarning($"MovementEvaluator: CSVデータなし ({nedoId})");
            yield break;
        }

        ParseCsvText(nedoId, csvText);

        if (_totalCsvFrames > 0)
        {
            _isEvaluating = true;
            Debug.Log($"MovementEvaluator: 評価開始 ({nedoId}, {_totalCsvFrames}フレーム)");
        }
    }

    /// <summary>
    /// 評価を停止し、最終スコアを算出する
    /// </summary>
    public void StopEvaluation()
    {
        _isEvaluating = false;
        CalculateFinalScore();
        CalculateDtwScore();
        Debug.Log($"MovementEvaluator: 評価終了 コサイン={FinalScore:F1} DTW={DtwScore:F1}");
    }

    // ========== ライフサイクル ==========

    void Update()
    {
        if (!_isEvaluating || poseEstimator == null || !poseEstimator.IsDetected)
            return;

        _elapsedTime += Time.deltaTime;
        int targetFrame = Mathf.FloorToInt(_elapsedTime * _frameRate);

        if (targetFrame >= _totalCsvFrames)
            return;

        // CSVの特徴量を取得
        float[] csvFeature = _csvFeatures[targetFrame];

        // リアルタイムランドマークから特徴量を計算
        float[] liveFeature = ComputeFeature(_activeNedoId, poseEstimator.LatestLandmarks);

        if (liveFeature == null)
            return;

        // コサイン類似度を算出
        float similarity = CosineSimilarity(csvFeature, liveFeature);

        // 0〜1にクランプ（負の類似度も考慮）
        float score = Mathf.Clamp01((similarity + 1f) / 2f);
        _frameScores.Add(score);

        // DTW用にリアルタイム特徴量を蓄積
        _liveFeatures.Add(liveFeature);

        if (logFrameScore)
        {
            Debug.Log($"MovementEvaluator: frame={targetFrame} 類似度={similarity:F3} スコア={score:F3}");
        }
    }

    // ========== CSV読み込み ==========

    /// <summary>
    /// CSVテキストをパースして特徴量リストに格納する
    /// </summary>
    private void ParseCsvText(string nedoId, string csvText)
    {
        _csvFeatures.Clear();
        var lines = csvText.Split('\n');

        // ヘッダースキップ
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            float[] feature = ParseCsvFeature(nedoId, values);

            if (feature != null)
            {
                _csvFeatures.Add(feature);
            }
        }

        _totalCsvFrames = _csvFeatures.Count;
    }

    private string GetCsvFileName(string nedoId)
    {
        switch (nedoId)
        {
            case "NEDO02": return "CSV/NEDO02.csv";
            case "NEDO06": return "CSV/NEDO06.csv";
            case "NEDO46": return "CSV/NEDO46.csv";
            default: return "";
        }
    }

    /// <summary>
    /// CSVの1行から特徴量ベクトルを抽出する
    /// </summary>
    private float[] ParseCsvFeature(string nedoId, string[] values)
    {
        switch (nedoId)
        {
            case "NEDO02":
                // frame, yaw, pitch, roll → [yaw, pitch, roll]
                if (values.Length >= 4)
                {
                    return new float[]
                    {
                        ParseFloat(values[1]),
                        ParseFloat(values[2]),
                        ParseFloat(values[3])
                    };
                }
                break;

            case "NEDO06":
                // frame, left_dx, left_dy, right_dx, right_dy → [left_dx, left_dy, right_dx, right_dy]
                if (values.Length >= 5)
                {
                    return new float[]
                    {
                        ParseFloat(values[1]),
                        ParseFloat(values[2]),
                        ParseFloat(values[3]),
                        ParseFloat(values[4])
                    };
                }
                break;

            case "NEDO46":
                // frame, shoulder_center_x, shoulder_center_y, shoulder_center_z → [x, y, z]
                if (values.Length >= 4)
                {
                    return new float[]
                    {
                        ParseFloat(values[1]),
                        ParseFloat(values[2]),
                        ParseFloat(values[3])
                    };
                }
                break;
        }
        return null;
    }

    // ========== ランドマーク → 特徴量変換 ==========

    /// <summary>
    /// NEDO IDに応じてランドマークから特徴量ベクトルを計算する
    /// </summary>
    private float[] ComputeFeature(string nedoId, IReadOnlyList<NormalizedLandmark> landmarks)
    {
        if (landmarks == null || landmarks.Count < 33)
            return null;

        switch (nedoId)
        {
            case "NEDO02": return ComputeYawPitchRoll(landmarks);
            case "NEDO06": return ComputeWristDelta(landmarks);
            case "NEDO46": return ComputeShoulderCenter(landmarks);
            default: return null;
        }
    }

    /// <summary>
    /// NEDO02用: 顔の向き（Yaw, Pitch, Roll）を計算する
    /// 鼻(0), 左耳(7), 右耳(8)のランドマークから推定
    /// </summary>
    private float[] ComputeYawPitchRoll(IReadOnlyList<NormalizedLandmark> landmarks)
    {
        var nose = landmarks[0];
        var leftEar = landmarks[7];
        var rightEar = landmarks[8];

        // 耳を結ぶベクトル（左→右）
        float earDx = rightEar.x - leftEar.x;
        float earDy = rightEar.y - leftEar.y;
        float earDz = rightEar.z - leftEar.z;

        // Yaw: 鼻と耳中点のX方向のずれから推定
        float earCenterX = (leftEar.x + rightEar.x) / 2f;
        float yaw = Mathf.Atan2(nose.x - earCenterX, Mathf.Abs(earDx)) * Mathf.Rad2Deg;

        // Pitch: 鼻と耳中点のY方向のずれから推定
        float earCenterY = (leftEar.y + rightEar.y) / 2f;
        float pitch = Mathf.Atan2(nose.y - earCenterY, Mathf.Abs(earDx)) * Mathf.Rad2Deg;

        // Roll: 両耳のY差から推定
        float roll = Mathf.Atan2(earDy, earDx) * Mathf.Rad2Deg;

        return new float[] { yaw, pitch, roll };
    }

    /// <summary>
    /// NEDO06用: 手首の相対位置を計算する
    /// 左手首(15), 右手首(16), 左肩(11), 右肩(12)
    /// </summary>
    private float[] ComputeWristDelta(IReadOnlyList<NormalizedLandmark> landmarks)
    {
        var leftWrist = landmarks[15];
        var rightWrist = landmarks[16];
        var leftShoulder = landmarks[11];
        var rightShoulder = landmarks[12];

        // 肩中心を基準にした手首の相対位置
        float shoulderCenterX = (leftShoulder.x + rightShoulder.x) / 2f;
        float shoulderCenterY = (leftShoulder.y + rightShoulder.y) / 2f;

        float leftDx = leftWrist.x - shoulderCenterX;
        float leftDy = leftWrist.y - shoulderCenterY;
        float rightDx = rightWrist.x - shoulderCenterX;
        float rightDy = rightWrist.y - shoulderCenterY;

        return new float[] { leftDx, leftDy, rightDx, rightDy };
    }

    /// <summary>
    /// NEDO46用: 肩の中心座標を計算する
    /// 左肩(11), 右肩(12)
    /// </summary>
    private float[] ComputeShoulderCenter(IReadOnlyList<NormalizedLandmark> landmarks)
    {
        var leftShoulder = landmarks[11];
        var rightShoulder = landmarks[12];

        float centerX = (leftShoulder.x + rightShoulder.x) / 2f;
        float centerY = (leftShoulder.y + rightShoulder.y) / 2f;
        float centerZ = (leftShoulder.z + rightShoulder.z) / 2f;

        return new float[] { centerX, centerY, centerZ };
    }

    // ========== スコア計算 ==========

    /// <summary>
    /// コサイン類似度を計算する
    /// </summary>
    private float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length || a.Length == 0)
            return 0f;

        float dot = 0f, magA = 0f, magB = 0f;

        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        float magnitude = Mathf.Sqrt(magA) * Mathf.Sqrt(magB);
        if (magnitude < 1e-8f)
            return 0f;

        return dot / magnitude;
    }

    /// <summary>
    /// 全フレームのスコアから最終スコア（0〜100）を算出する
    /// </summary>
    private void CalculateFinalScore()
    {
        if (_frameScores.Count == 0)
        {
            FinalScore = 0f;
            return;
        }

        float sum = 0f;
        foreach (var s in _frameScores)
        {
            sum += s;
        }

        FinalScore = (sum / _frameScores.Count) * 100f;
    }

    // ========== DTW計算 ==========

    /// <summary>
    /// DTW（Dynamic Time Warping）スコアを算出する
    /// 時系列のタイミングずれに対してロバストな比較手法
    /// </summary>
    private void CalculateDtwScore()
    {
        if (_csvFeatures.Count == 0 || _liveFeatures.Count == 0)
        {
            DtwScore = 0f;
            return;
        }

        int n = _csvFeatures.Count;
        int m = _liveFeatures.Count;

        // DTWコスト行列（メモリ節約のため2行のみ保持）
        float[] prevRow = new float[m + 1];
        float[] currRow = new float[m + 1];

        // 初期化
        for (int j = 0; j <= m; j++)
            prevRow[j] = float.MaxValue;
        prevRow[0] = 0f;

        for (int i = 1; i <= n; i++)
        {
            currRow[0] = float.MaxValue;

            for (int j = 1; j <= m; j++)
            {
                float cost = EuclideanDistance(_csvFeatures[i - 1], _liveFeatures[j - 1]);
                currRow[j] = cost + Mathf.Min(
                    prevRow[j],         // 挿入
                    currRow[j - 1],     // 削除
                    prevRow[j - 1]      // 一致
                );
            }

            // 行を入れ替え
            (prevRow, currRow) = (currRow, prevRow);
        }

        float dtwDistance = prevRow[m];
        int pathLength = Mathf.Max(n, m);
        float avgDistance = dtwDistance / pathLength;

        // DTW距離をスコアに変換（距離が小さいほどスコアが高い）
        // exp(-distance) で 0〜1 に変換し、× 100
        DtwScore = Mathf.Exp(-avgDistance) * 100f;
    }

    /// <summary>
    /// ユークリッド距離を計算する
    /// </summary>
    private float EuclideanDistance(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            return float.MaxValue;

        float sum = 0f;
        for (int i = 0; i < a.Length; i++)
        {
            float diff = a[i] - b[i];
            sum += diff * diff;
        }
        return Mathf.Sqrt(sum);
    }

    // ========== ユーティリティ ==========

    private float ParseFloat(string s)
    {
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
            return val;
        return 0f;
    }
}
