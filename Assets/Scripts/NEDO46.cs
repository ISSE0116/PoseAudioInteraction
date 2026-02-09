using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 肩の位置データに基づいて音源を左右に移動させる
/// CSVファイルから肩のX座標を読み込み、リスナー前面で左右移動
/// </summary>
public class NEDO46 : NEDOBase
{
    // ========== 固有のInspector設定 ==========
    
    [Header("Targets")]
    public Transform soundSource;   // 動かす音源Transform
    public Transform marker;        // 可視化用マーカー（任意）

    [Header("UI")]
    public Button nedo46Button;     // 再生開始ボタン

    [Header("Motion")]
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

    // ========== 内部変数 ==========
    
    private struct FrameX
    {
        public float x;   // normalized 0..1
        public bool valid;
    }

    private readonly List<FrameX> xSeries = new();
    private Vector3 targetPos;

    // ========== 初期化 ==========

    protected override void Start()
    {
        csvFileName = "CSV/NEDO46.csv";
        base.Start();
        
        if (soundSource != null)
            targetPos = soundSource.position;
    }

    // ========== 抽象メソッドの実装 ==========

    protected override void LoadCsv()
    {
        xSeries.Clear();
        string path = GetCsvPath();

        if (!System.IO.File.Exists(path))
        {
            Debug.LogError($"NEDO46: CSVが見つかりません: {path}");
            return;
        }

        var lines = System.IO.File.ReadAllLines(path);
        int start = (skipHeader && lines.Length > 0) ? 1 : 0;

        for (int i = start; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var cols = lines[i].Split(',');
            if (cols.Length < 2) continue;

            bool okX = TryParseFloat(cols[1], out float x);
            xSeries.Add(new FrameX { x = x, valid = okX });
        }
    }

    protected override void UpdatePosition(int frame)
    {
        if (soundSource == null) return;

        var fx = xSeries[frame];
        if (!fx.valid)
        {
            if (ignoreMissing) return;
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

        // 音源を常にリスナーへ向ける
        soundSource.forward = (listener.position - soundSource.position).normalized;

        if (marker != null)
            marker.position = soundSource.position;
    }

    protected override void SetupButton()
    {
        SetupButtonListener(nedo46Button, "nedo46Button");
    }

    protected override int GetLoadedFrameCount()
    {
        return xSeries.Count;
    }
}
