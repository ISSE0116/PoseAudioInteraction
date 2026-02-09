using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 顔の向き（Yaw角度）データに基づいて音源を円周上で移動させる
/// CSVファイルからフレームごとのYaw角度を読み込み、リスナーの周りを回転
/// </summary>
public class NEDO02 : MonoBehaviour
{
    // ========== Inspector設定 ==========
    
    [Header("Targets")]
    public Transform soundSource;   // 動かす音源Transform
    public Transform listener;      // リスナー（耳の位置）
    public Transform marker;        // 可視化用マーカー（任意）

    [Header("Audio")]
    public AudioSource audioSource; // 音声再生用

    [Header("UI")]
    public Button nedo02Button;     // 再生開始ボタン

    [Header("CSV")]
    public string csvFileName = "CSV/face_orientation.csv";

    [Header("Playback")]
    public float radius = 10f;      // リスナーからの距離（m）
    public float frameRate = 60f;   // CSVのフレームレート
    public bool loop = false;       // ループ再生

    // ========== 内部変数 ==========
    
    private readonly List<float> yawList = new();
    private float elapsedTime = 0f;
    private int currentFrame = 0;
    private bool isPlaying = false;

    // ========== ライフサイクル ==========

    void Start()
    {
        LoadCsv();

        if (nedo02Button != null)
        {
            nedo02Button.onClick.AddListener(StartPlayback);
            Debug.Log("NEDO02: ボタンにリスナーを追加しました");
        }
        else
        {
            Debug.LogWarning("NEDO02: ボタンが未設定です");
        }

        Debug.Log($"NEDO02: {yawList.Count} フレーム読み込み完了");
    }

    void Update()
    {
        if (!isPlaying || soundSource == null || listener == null || yawList.Count == 0)
            return;

        elapsedTime += Time.deltaTime;
        int targetFrame = Mathf.FloorToInt(elapsedTime * frameRate);

        if (targetFrame < yawList.Count)
        {
            float yaw = yawList[targetFrame];

            // Yaw角度に基づいて円周上の位置を計算
            float rad = yaw * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(
                listener.position.x + radius * Mathf.Sin(rad),
                soundSource.position.y,
                listener.position.z + radius * Mathf.Cos(rad)
            );

            soundSource.position = pos;
            soundSource.forward = (listener.position - pos).normalized;

            // マーカーも更新
            if (marker != null)
                marker.position = soundSource.position;

            currentFrame = targetFrame;
        }
        else
        {
            // 再生終了
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
                audioSource?.Stop();
                Debug.Log("NEDO02: 再生終了");
            }
        }
    }

    // ========== 公開メソッド ==========

    public void StartPlayback()
    {
        if (yawList.Count == 0)
        {
            Debug.LogWarning("NEDO02: CSVデータが読み込まれていません");
            return;
        }

        elapsedTime = 0f;
        currentFrame = 0;
        isPlaying = true;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.time = 0f;
            audioSource.Play();
        }

        Debug.Log("NEDO02: 再生開始");
    }

    // ========== 内部メソッド ==========

    private void LoadCsv()
    {
        yawList.Clear();
        string path = Path.Combine(Application.streamingAssetsPath, csvFileName);

        if (!File.Exists(path))
        {
            Debug.LogError($"NEDO02: CSVが見つかりません: {path}");
            return;
        }

        using (StreamReader reader = new StreamReader(path))
        {
            reader.ReadLine(); // ヘッダーをスキップ

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');

                if (values.Length >= 2 &&
                    float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float yaw))
                {
                    yawList.Add(yaw);
                }
            }
        }
    }
}