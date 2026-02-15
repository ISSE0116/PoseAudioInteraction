using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Webカメラの映像をRawImageに表示するクラス
/// </summary>
public class WebCamDisplay : MonoBehaviour
{
    [Header("Display Target")]
    public RawImage displayImage;
    public AspectRatioFitter fitter;

    private WebCamTexture webCamTexture;

    void Start()
    {
        if (displayImage == null)
        {
            Debug.LogError("WebCamDisplay: Display Image is not assigned.");
            return;
        }

        StartWebCam();
    }

    void StartWebCam()
    {
        // カメラデバイスの取得
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogWarning("WebCamDisplay: No camera detected.");
            return;
        }

        // 最初のカメラを使用（通常はインカメまたはデフォルトカメラ）
        // 必要に応じてデバイス名でフィルタリング可能
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

    void OnDestroy()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
        }
    }
}
