using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ホーム画面のUI管理クラス
/// </summary>
public class HomeManager : MonoBehaviour
{
    [Header("Training Select Buttons")]
    public Button btnNedo02; // 顔向き
    public Button btnNedo06; // 手首
    public Button btnNedo46; // 肩

    void Start()
    {
        if (btnNedo02 != null)
            btnNedo02.onClick.AddListener(() => OnSelectTraining("NEDO02"));
        
        if (btnNedo06 != null)
            btnNedo06.onClick.AddListener(() => OnSelectTraining("NEDO06"));
        
        if (btnNedo46 != null)
            btnNedo46.onClick.AddListener(() => OnSelectTraining("NEDO46"));
    }

    private void OnSelectTraining(string nedoId)
    {
        Debug.Log($"Selected Training: {nedoId}");
        SceneNavigator.Instance.LoadTraining(nedoId);
    }
}
