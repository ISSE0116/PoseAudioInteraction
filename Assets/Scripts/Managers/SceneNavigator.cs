using UnityEngine;

/// <summary>
/// Canvas切り替えによる画面遷移を管理するクラス
/// シングルトンとして存在し、各Canvasの表示/非表示を制御する
/// </summary>
public class SceneNavigator : MonoBehaviour
{
    public static SceneNavigator Instance { get; private set; }

    // 選択されたトレーニングID（例: "NEDO02", "NEDO06", "NEDO46"）
    public string SelectedNedoId { get; set; }

    // トレーニングの評価スコア（0〜100）
    public float LastScore { get; set; }

    // DTWスコア（0〜100）
    public float LastDtwScore { get; set; }

    [Header("Canvas References")]
    public GameObject canvasHome;
    public GameObject canvasTraining;
    public GameObject canvasResult;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 初期状態: ホーム画面を表示
        LoadHome();
    }

    /// <summary>
    /// ホーム画面を表示
    /// </summary>
    public void LoadHome()
    {
        ShowCanvas(canvasHome);
        Debug.Log("画面切替: ホーム");
    }

    /// <summary>
    /// トレーニング画面を表示
    /// </summary>
    /// <param name="nedoId">選択したトレーニングID</param>
    public void LoadTraining(string nedoId)
    {
        SelectedNedoId = nedoId;
        ShowCanvas(canvasTraining);
        Debug.Log($"画面切替: トレーニング ({nedoId})");
    }

    /// <summary>
    /// 結果画面を表示
    /// </summary>
    public void LoadResult()
    {
        ShowCanvas(canvasResult);
        Debug.Log("画面切替: リザルト");
    }

    /// <summary>
    /// 指定したCanvasのみを表示し、他を非表示にする
    /// </summary>
    private void ShowCanvas(GameObject target)
    {
        if (canvasHome != null) canvasHome.SetActive(target == canvasHome);
        if (canvasTraining != null) canvasTraining.SetActive(target == canvasTraining);
        if (canvasResult != null) canvasResult.SetActive(target == canvasResult);
    }
}
