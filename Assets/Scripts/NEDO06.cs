using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 両手首の位置データに基づいて2つの音源を移動させる
/// CSVファイルから左右の手首オフセットを読み込み、リスナー前面で移動
/// </summary>
public class NEDO06 : NEDOBase
{
    // ========== 固有のInspector設定 ==========
    
    [Header("Targets")]
    public Transform leftSoundSource;   // 左音源
    public Transform rightSoundSource;  // 右音源

    [Header("Audio (Additional)")]
    public AudioSource leftAudioSource;  // 左音声再生用
    public AudioSource rightAudioSource; // 右音声再生用

    [Header("UI")]
    public Button nedo06Button;          // 再生開始ボタン

    [Header("Motion")]
    public float scale = 3.5f;           // 位置のスケール倍率
    public float verticalOffset = 1.5f;  // 高さオフセット（m）

    // ========== 内部変数 ==========
    
    private readonly List<Vector2> leftOffsets = new();
    private readonly List<Vector2> rightOffsets = new();

    // ========== 初期化 ==========

    protected override void Start()
    {
        csvFileName = "CSV/NEDO06.csv";
        frameRate = 30f; // このCSVは30fps
        base.Start();
    }

    // ========== 抽象メソッドの実装 ==========

    protected override void LoadCsv()
    {
        leftOffsets.Clear();
        rightOffsets.Clear();
        string path = GetCsvPath();

        if (!System.IO.File.Exists(path))
        {
            Debug.LogError($"NEDO06: CSVが見つかりません: {path}");
            return;
        }

        using (var reader = new System.IO.StreamReader(path))
        {
            if (skipHeader) reader.ReadLine();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');

                if (values.Length >= 5 &&
                    TryParseFloat(values[1], out float ldx) &&
                    TryParseFloat(values[2], out float ldy) &&
                    TryParseFloat(values[3], out float rdx) &&
                    TryParseFloat(values[4], out float rdy))
                {
                    leftOffsets.Add(new Vector2(ldx, ldy));
                    rightOffsets.Add(new Vector2(rdx, rdy));
                }
            }
        }
    }

    protected override void UpdatePosition(int frame)
    {
        if (leftSoundSource == null || rightSoundSource == null) return;

        Vector2 leftOffset = leftOffsets[frame];
        Vector2 rightOffset = rightOffsets[frame];
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
    }

    protected override void SetupButton()
    {
        SetupButtonListener(nedo06Button, "nedo06Button");
    }

    protected override int GetLoadedFrameCount()
    {
        return Mathf.Min(leftOffsets.Count, rightOffsets.Count);
    }

    // ========== オーバーライド ==========

    public override void StartPlayback()
    {
        base.StartPlayback();
        leftAudioSource?.Play();
        rightAudioSource?.Play();
    }

    protected override void OnPlaybackEnd()
    {
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
            Debug.Log($"{GetType().Name}: 再生終了");
        }
    }
}