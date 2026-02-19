using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 画面サイズに応じてUIを自動調整するスクリプト
/// PC とモバイルで最適なUI表示を実現する
/// </summary>
[RequireComponent(typeof(CanvasScaler))]
public class ResponsiveUIScaler : MonoBehaviour
{
    [Header("Reference Resolutions")]
    [Tooltip("PC用の基準解像度")]
    public Vector2 pcReferenceResolution = new(1920, 1080);

    [Tooltip("モバイル用の基準解像度")]
    public Vector2 mobileReferenceResolution = new(1080, 1920);

    [Header("Button Settings")]
    [Tooltip("モバイル時にボタンの最小サイズを拡大する")]
    public float mobileMinButtonSize = 80f;

    [Tooltip("対象のボタン一覧（任意）")]
    public Button[] targetButtons;

    private CanvasScaler _canvasScaler;

    void Awake()
    {
        _canvasScaler = GetComponent<CanvasScaler>();
        ApplyResponsiveSettings();
    }

    /// <summary>
    /// プラットフォームに応じたUI設定を適用する
    /// </summary>
    private void ApplyResponsiveSettings()
    {
        if (_canvasScaler == null) return;

        bool isMobile = IsMobilePlatform();

        // CanvasScaler の Reference Resolution を切替
        _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _canvasScaler.referenceResolution = isMobile ? mobileReferenceResolution : pcReferenceResolution;
        _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        _canvasScaler.matchWidthOrHeight = isMobile ? 0.5f : 1f;

        Debug.Log($"ResponsiveUIScaler: {(isMobile ? "モバイル" : "PC")}モード " +
                  $"解像度={_canvasScaler.referenceResolution}");

        // モバイルではボタンの最小サイズを保証
        if (isMobile)
        {
            EnlargeButtonsForTouch();
        }
    }

    /// <summary>
    /// タッチ操作向けにボタンサイズを拡大する
    /// </summary>
    private void EnlargeButtonsForTouch()
    {
        if (targetButtons == null) return;

        foreach (var button in targetButtons)
        {
            if (button == null) continue;

            var rt = button.GetComponent<RectTransform>();
            if (rt == null) continue;

            // 最小サイズを保証
            var size = rt.sizeDelta;
            if (size.x < mobileMinButtonSize)
                size.x = mobileMinButtonSize;
            if (size.y < mobileMinButtonSize)
                size.y = mobileMinButtonSize;
            rt.sizeDelta = size;
        }
    }

    /// <summary>
    /// モバイルプラットフォームかどうか判定する
    /// </summary>
    private bool IsMobilePlatform()
    {
#if UNITY_ANDROID || UNITY_IOS
        return true;
#else
        return false;
#endif
    }
}
