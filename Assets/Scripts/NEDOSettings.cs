using UnityEngine;

/// <summary>
/// NEDO系スクリプトの共通設定を管理するScriptableObject
/// Assets/Settings/NEDOSettings.asset として作成して使用
/// </summary>
[CreateAssetMenu(fileName = "NEDOSettings", menuName = "NEDO/Settings", order = 1)]
public class NEDOSettings : ScriptableObject
{
    [Header("Playback Defaults")]
    [Tooltip("デフォルトのフレームレート")]
    public float defaultFrameRate = 60f;
    
    [Tooltip("デフォルトでループ再生を有効にするか")]
    public bool defaultLoop = false;

    [Header("NEDO02 - Yaw Control")]
    [Tooltip("リスナーからの距離（m）")]
    public float nedo02Radius = 10f;

    [Header("NEDO06 - Wrist Control")]
    [Tooltip("位置のスケール倍率")]
    public float nedo06Scale = 3.5f;
    
    [Tooltip("高さオフセット（m）")]
    public float nedo06VerticalOffset = 1.5f;
    
    [Tooltip("フレームレート（30fps）")]
    public float nedo06FrameRate = 30f;

    [Header("NEDO46 - Shoulder Control")]
    [Tooltip("目の前の距離（m）")]
    public float nedo46ForwardDistance = 2.0f;
    
    [Tooltip("左右の最大移動幅（m）")]
    public float nedo46LateralRange = 1.5f;
    
    [Tooltip("スムージング係数")]
    public float nedo46Smooth = 12f;
    
    [Tooltip("スムージングを有効にするか")]
    public bool nedo46EnableSmoothing = true;

    [Header("CSV Paths")]
    [Tooltip("CSVファイルのベースパス（StreamingAssets/からの相対パス）")]
    public string csvBasePath = "CSV/";
    
    public string nedo02CsvFile = "face_orientation.csv";
    public string nedo06CsvFile = "relative_wrist_to_nose.csv";
    public string nedo46CsvFile = "shoulder_center.csv";

    // ========== ヘルパーメソッド ==========

    /// <summary>
    /// 完全なCSVパスを取得
    /// </summary>
    public string GetFullCsvPath(string fileName)
    {
        return csvBasePath + fileName;
    }
}
