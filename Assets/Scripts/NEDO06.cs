using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 両手首の位置データに基づいて2つの音源を移動させる
/// CSVファイルから左右の手首オフセットを読み込み、リスナー前面で移動
/// </summary>
public class NEDO06 : MonoBehaviour
{
    // ========== Inspector設定 ==========
    
    [Header("Targets")]
    public Transform leftSoundSource;   // 左音源
    public Transform rightSoundSource;  // 右音源
    public Transform listener;          // リスナー（耳の位置）

    [Header("Audio")]
    public AudioSource leftAudioSource;  // 左音声再生用
    public AudioSource rightAudioSource; // 右音声再生用

    [Header("UI")]
    public Button nedo06Button;          // 再生開始ボタン ※修正: nedo02Button → nedo06Button

    [Header("CSV")]
    public string csvFileName = "CSV/relative_wrist_to_nose.csv";

    [Header("Playback")]
    public float frameRate = 30f;        // CSVのフレームレート
    public bool loop = false;            // ループ再生

    [Header("Motion")]
    public float scale = 3.5f;           // 位置のスケール倍率
    public float verticalOffset = 1.5f;  // 高さオフセット（m）

    // ========== 内部変数 ==========
    
    private readonly List<Vector2> leftOffsets = new();
    private readonly List<Vector2> rightOffsets = new();
    private float elapsedTime = 0f;
    private int currentFrame = 0;
    private bool isPlaying = false;

    // ========== ライフサイクル ==========

    void Start()
    {
        LoadCsv();

        if (nedo06Button != null)
        {
            nedo06Button.onClick.AddListener(StartPlayback);
            Debug.Log("NEDO06: ボタンにリスナーを追加しました");
        }
        else
        {
            Debug.LogWarning("NEDO06: ボタンが未設定です");
        }

        Debug.Log($"NEDO06: {leftOffsets.Count} フレーム読み込み完了");
    }

    void Update()
    {
        if (!isPlaying || leftSoundSource == null || rightSoundSource == null || listener == null)
            return;

        if (leftOffsets.Count == 0 || rightOffsets.Count == 0)
            return;

        elapsedTime += Time.deltaTime;
        int targetFrame = Mathf.FloorToInt(elapsedTime * frameRate);

        if (targetFrame < leftOffsets.Count && targetFrame < rightOffsets.Count)
        {
            Vector2 leftOffset = leftOffsets[targetFrame];
            Vector2 rightOffset = rightOffsets[targetFrame];
            Vector3 listenerPos = listener.position;

            // 左右の音源位置を更新（左右が入れ替わっている点に注意）
            rightSoundSource.position = new Vector3(
                listenerPos.x + leftOffset.x * scale,
                listenerPos.y - leftOffset.y * scale + verticalOffset,
                listenerPos.z
            );

            leftSoundSource.position = new Vector3(
                listenerPos.x + rightOffset.x * scale,
                listenerPos.y - rightOffset.y * scale + verticalOffset,
                listenerPos.z
            );

            // 音源をリスナーに向ける
            leftSoundSource.forward = (listenerPos - leftSoundSource.position).normalized;
            rightSoundSource.forward = (listenerPos - rightSoundSource.position).normalized;

            currentFrame = targetFrame;
        }
        else
        {
            // 再生終了
            if (loop)
            {
                elapsedTime = 0f;
                currentFrame = 0;
                leftAudioSource?.Play();
                rightAudioSource?.Play();
            }
            else
            {
                isPlaying = false;
                leftAudioSource?.Stop();
                rightAudioSource?.Stop();
                Debug.Log("NEDO06: 再生終了");
            }
        }
    }

    // ========== 公開メソッド ==========

    public void StartPlayback()
    {
        if (leftOffsets.Count == 0 || rightOffsets.Count == 0)
        {
            Debug.LogWarning("NEDO06: CSVデータが読み込まれていません");
            return;
        }

        elapsedTime = 0f;
        currentFrame = 0;
        isPlaying = true;

        leftAudioSource?.Play();
        rightAudioSource?.Play();

        Debug.Log("NEDO06: 再生開始");
    }

    // ========== 内部メソッド ==========

    private void LoadCsv()
    {
        leftOffsets.Clear();
        rightOffsets.Clear();
        string path = Path.Combine(Application.streamingAssetsPath, csvFileName);

        if (!File.Exists(path))
        {
            Debug.LogError($"NEDO06: CSVが見つかりません: {path}");
            return;
        }

        using (StreamReader reader = new StreamReader(path))
        {
            reader.ReadLine(); // ヘッダーをスキップ

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');

                if (values.Length >= 5 &&
                    float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float ldx) &&
                    float.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float ldy) &&
                    float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float rdx) &&
                    float.TryParse(values[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float rdy))
                {
                    leftOffsets.Add(new Vector2(ldx, ldy));
                    rightOffsets.Add(new Vector2(rdx, rdy));
                }
            }
        }
    }
}