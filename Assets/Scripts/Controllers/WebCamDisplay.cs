using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Webカメラの映像をRawImageに表示するクラス
/// Canvas切り替えに対応（OnEnable/OnDisable）
/// </summary>
public class WebCamDisplay : MonoBehaviour
{
    [Header("Display Target")]
    public RawImage displayImage;
    public AspectRatioFitter fitter;

    private WebCamTexture webCamTexture;

    void OnEnable()
    {
        if (displayImage == null)
        {
            Debug.LogError("WebCamDisplay: Display Image is not assigned.");
            return;
        }

        StartWebCam();
    }

    void OnDisable()
    {
        StopWebCam();
    }

    /// <summary>
    /// Webカメラを開始する
    /// </summary>
    public void StartWebCam()
    {
        // 既に起動中なら何もしない
        if (webCamTexture != null && webCamTexture.isPlaying)
            return;

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogWarning("WebCamDisplay: カメラが検出されませんでした。");
            return;
        }

        // 最初のカメラを使用
        webCamTexture = new WebCamTexture(devices[0].name, 1280, 720, 30);

        displayImage.texture = webCamTexture;
        webCamTexture.Play();

        Debug.Log($"WebCam Started: {devices[0].name} ({webCamTexture.width}x{webCamTexture.height})");

        // アスペクト比の調整
        if (fitter != null)
        {
            fitter.aspectRatio = (float)webCamTexture.width / webCamTexture.height;
        }
    }

    /// <summary>
    /// Webカメラを停止する
    /// </summary>
    public void StopWebCam()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            Destroy(webCamTexture);
            webCamTexture = null;

            if (displayImage != null)
                displayImage.texture = null;
        }
    }

    /// <summary>
    /// WebCamTextureを外部に公開する（PoseEstimator等で使用）
    /// </summary>
    public WebCamTexture GetWebCamTexture()
    {
        return webCamTexture;
    }

    void OnDestroy()
    {
        StopWebCam();
    }
}
