using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// NEDO系スクリプトの基底クラス
/// CSV読み込み、再生制御、ボタン設定などの共通処理を提供
/// </summary>
public abstract class NEDOBase : MonoBehaviour
{
    // ========== 共通Inspector設定 ==========
    
    [Header("Listener")]
    public Transform listener;          // リスナー（耳の位置）

    [Header("Audio")]
    public AudioSource audioSource;     // 音声再生用

    [Header("CSV")]
    public string csvFileName;          // CSVファイル名（StreamingAssets/CSV/からの相対パス）
    public bool skipHeader = true;      // ヘッダー行をスキップするか

    [Header("Playback")]
    public float frameRate = 60f;       // CSVのフレームレート
    public bool loop = false;           // ループ再生

    // ========== コールバック ==========

    /// <summary>
    /// 再生完了時に呼ばれるコールバック
    /// </summary>
    public System.Action onPlaybackComplete;

    // ========== 内部変数 ==========
    
    protected float elapsedTime = 0f;
    protected int currentFrame = 0;
    protected bool isPlaying = false;
    protected int totalFrames = 0;

    // ========== 抽象メソッド（派生クラスで実装） ==========

    /// <summary>
    /// CSVデータを読み込む
    /// </summary>
    protected abstract void LoadCsv();

    /// <summary>
    /// 指定フレームの位置を更新する
    /// </summary>
    protected abstract void UpdatePosition(int frame);

    /// <summary>
    /// ボタンを設定する（派生クラスのButton変数を使用）
    /// </summary>
    protected abstract void SetupButton();

    /// <summary>
    /// 読み込んだフレーム数を返す
    /// </summary>
    protected abstract int GetLoadedFrameCount();

    // ========== ライフサイクル ==========

    protected virtual void Start()
    {
        LoadCsv();
        SetupButton();
        totalFrames = GetLoadedFrameCount();
        Debug.Log($"{GetType().Name}: {totalFrames} フレーム読み込み完了");
    }

    protected virtual void Update()
    {
        if (!isPlaying || listener == null || totalFrames == 0)
            return;

        elapsedTime += Time.deltaTime;
        int targetFrame = Mathf.FloorToInt(elapsedTime * frameRate);

        if (targetFrame < totalFrames)
        {
            UpdatePosition(targetFrame);
            currentFrame = targetFrame;
        }
        else
        {
            OnPlaybackEnd();
        }
    }

    // ========== 公開メソッド ==========

    /// <summary>
    /// 再生を開始する
    /// </summary>
    public virtual void StartPlayback()
    {
        if (totalFrames == 0)
        {
            Debug.LogWarning($"{GetType().Name}: CSVデータが読み込まれていません");
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

        Debug.Log($"{GetType().Name}: 再生開始");
    }

    /// <summary>
    /// 再生を停止する
    /// </summary>
    public virtual void StopPlayback()
    {
        isPlaying = false;
        audioSource?.Stop();
        Debug.Log($"{GetType().Name}: 再生停止");
    }

    // ========== 内部メソッド ==========

    /// <summary>
    /// 再生終了時の処理
    /// </summary>
    protected virtual void OnPlaybackEnd()
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
            audioSource?.Stop();
            Debug.Log($"{GetType().Name}: 再生終了");
            onPlaybackComplete?.Invoke();
        }
    }

    /// <summary>
    /// CSVファイルのフルパスを取得する
    /// </summary>
    protected string GetCsvPath()
    {
        return Path.Combine(Application.streamingAssetsPath, csvFileName);
    }

    /// <summary>
    /// 文字列をfloatにパースする（None対応）
    /// </summary>
    protected bool TryParseFloat(string s, out float value)
    {
        if (string.IsNullOrEmpty(s) || s.Trim().Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            value = 0f;
            return false;
        }
        return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>
    /// ボタンにリスナーを追加するヘルパーメソッド
    /// </summary>
    protected void SetupButtonListener(Button button, string buttonName)
    {
        if (button != null)
        {
            button.onClick.AddListener(StartPlayback);
            Debug.Log($"{GetType().Name}: {buttonName}にリスナーを追加しました");
        }
        else
        {
            Debug.LogWarning($"{GetType().Name}: {buttonName}が未設定です");
        }
    }
}
