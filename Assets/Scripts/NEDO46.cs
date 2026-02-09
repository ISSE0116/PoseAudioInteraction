using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class NEDO46 : MonoBehaviour
{
    [Header("Targets")]
    public Transform soundSource;   // 動かす音源Transform
    public Transform listener;      // 耳（MainCamera等）
    public Transform marker;        // 任意（可視化用）
    public AudioSource audioSource; // 任意（同期再生）

    [Header("UI")]
    public Button nedo46Button;     // ★NEDO46ボタン

    [Header("CSV (StreamingAssets)")]
    public string csvFileName = "CSV/shoulder_center.csv";
    public bool skipHeader = true;

    [Header("Playback")]
    public float frameRate = 60f;   // 元動画FPSに合わせる
    public bool loop = false;

    [Header("Motion (listener local space)")]
    public float forwardDistance = 2.0f;  // 目の前の距離（m）
    public float lateralRange = 1.5f;     // 左右の最大移動幅（m）
    public bool mirrorX = false;          // 左右が逆ならtrue

    [Header("Height")]
    public bool useEarHeight = true;      // true: listenerの高さ
    public float fixedHeightY = 1.6f;     // useEarHeight=falseのときのY

    [Header("Smoothing")]
    public bool enableSmoothing = true;
    public float smooth = 12f;
    public bool ignoreMissing = true;     // 欠損フレームは更新しない

    // ---- internal ----
    private struct FrameX
    {
        public float x;   // normalized 0..1
        public bool valid;
    }

    private readonly List<FrameX> xSeries = new();
    private float elapsedTime = 0f;
    private int currentFrame = 0;
    private bool isPlaying = false;

    private Vector3 targetPos;

    void Start()
    {
        LoadCsvX();

        if (nedo46Button != null)
        {
            nedo46Button.onClick.AddListener(StartPlayback);
            Debug.Log("NEDO46ボタンにリスナーを追加しました");
        }
        else
        {
            Debug.LogWarning("nedo46Button が未設定です。ボタンから開始したい場合はInspectorで設定してください。");
        }

        if (soundSource != null)
            targetPos = soundSource.position;

        Debug.Log($"CSV x frames loaded: {xSeries.Count}");
    }

    void Update()
    {
        if (!isPlaying || soundSource == null || listener == null || xSeries.Count == 0)
            return;

        elapsedTime += Time.deltaTime;
        currentFrame = Mathf.FloorToInt(elapsedTime * frameRate);

        if (currentFrame < 0) currentFrame = 0;

        if (currentFrame >= xSeries.Count)
        {
            if (loop)
            {
                elapsedTime = 0f;
                currentFrame = 0;

                if (audioSource != null)
                {
                    audioSource.time = 0f;
                    audioSource.Play();
                }
            }
            else
            {
                isPlaying = false;
                return;
            }
        }

        var fx = xSeries[currentFrame];
        if (!fx.valid)
        {
            if (ignoreMissing) return;
            // 欠損でも何かしたい場合はここで補間など
            return;
        }

        // normalized x (0..1) -> centered (-0.5..+0.5)
        float nx = fx.x - 0.5f;
        if (mirrorX) nx *= -1f;

        // listenerローカル空間で「目の前（forwardDistance）」に置き、左右だけ動かす
        Vector3 basePos = listener.position + listener.forward * forwardDistance;

        // 高さは耳の高さ固定（listenerのY） or 任意固定
        float y = useEarHeight ? listener.position.y : fixedHeightY;

        Vector3 pos = basePos + listener.right * (nx * lateralRange);
        pos.y = y;

        targetPos = pos;

        if (enableSmoothing)
        {
            soundSource.position = Vector3.Lerp(
                soundSource.position,
                targetPos,
                1f - Mathf.Exp(-smooth * Time.deltaTime)
            );
        }
        else
        {
            soundSource.position = targetPos;
        }

        // 音源を常にリスナーへ向ける（必要なら）
        soundSource.forward = (listener.position - soundSource.position).normalized;

        if (marker != null)
            marker.position = soundSource.position;
    }

    public void StartPlayback()
    {
        elapsedTime = 0f;
        currentFrame = 0;
        isPlaying = true;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.time = 0f;
            audioSource.Play();
        }

        Debug.Log("NEDO46: CSV(X) 追従の再生を開始しました");
    }

    private void LoadCsvX()
    {
        xSeries.Clear();

        string path = Path.Combine(Application.streamingAssetsPath, csvFileName);

        if (!File.Exists(path))
        {
            Debug.LogError($"CSV not found: {path}\nAssets/StreamingAssets/{csvFileName} に置いてください。");
            return;
        }

        var lines = File.ReadAllLines(path);
        int start = (skipHeader && lines.Length > 0) ? 1 : 0;

        for (int i = start; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var cols = lines[i].Split(',');
            // frame, shoulder_center_x, shoulder_center_y, shoulder_center_z
            if (cols.Length < 2) continue;

            bool okX = TryParseFloat(cols[1], out float x);

            xSeries.Add(new FrameX
            {
                x = x,
                valid = okX
            });
        }
    }

    private bool TryParseFloat(string s, out float v)
    {
        if (string.IsNullOrEmpty(s) || s.Trim().Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            v = 0f;
            return false;
        }
        return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
    }
}
