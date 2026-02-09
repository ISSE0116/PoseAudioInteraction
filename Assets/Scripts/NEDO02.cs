using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 顔の向き（Yaw角度）データに基づいて音源を円周上で移動させる
/// CSVファイルからフレームごとのYaw角度を読み込み、リスナーの周りを回転
/// </summary>
public class NEDO02 : NEDOBase
{
    // ========== 固有のInspector設定 ==========
    
    [Header("Targets")]
    public Transform soundSource;   // 動かす音源Transform
    public Transform marker;        // 可視化用マーカー（任意）

    [Header("UI")]
    public Button nedo02Button;     // 再生開始ボタン

    [Header("Motion")]
    public float radius = 10f;      // リスナーからの距離（m）

    // ========== 内部変数 ==========
    
    private readonly List<float> yawList = new();

    // ========== 初期化 ==========

    protected override void Start()
    {
        csvFileName = "CSV/NEDO02.csv";
        base.Start();
    }

    // ========== 抽象メソッドの実装 ==========

    protected override void LoadCsv()
    {
        yawList.Clear();
        string path = GetCsvPath();

        if (!System.IO.File.Exists(path))
        {
            Debug.LogError($"NEDO02: CSVが見つかりません: {path}");
            return;
        }

        using (var reader = new System.IO.StreamReader(path))
        {
            if (skipHeader) reader.ReadLine();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');

                if (values.Length >= 2 && TryParseFloat(values[1], out float yaw))
                {
                    yawList.Add(yaw);
                }
            }
        }
    }

    protected override void UpdatePosition(int frame)
    {
        if (soundSource == null) return;

        float yaw = yawList[frame];

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
    }

    protected override void SetupButton()
    {
        SetupButtonListener(nedo02Button, "nedo02Button");
    }

    protected override int GetLoadedFrameCount()
    {
        return yawList.Count;
    }
}