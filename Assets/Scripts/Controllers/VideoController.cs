using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// VideoPlayerを制御するクラス
/// 元動画の再生を担当し、RawImageに映像を表示する
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class VideoController : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    [Header("Display")]
    public RawImage displayImage;       // 映像表示先のRawImage

    [Header("Settings")]
    public bool playOnStart = false;
    public string videoFileName;        // StreamingAssets内のファイル名

    private RenderTexture renderTexture;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    void OnEnable()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // 動画パスの設定
        if (!string.IsNullOrEmpty(videoFileName))
        {
            SetVideoFile(videoFileName);
        }

        if (playOnStart)
        {
            Play();
        }
    }

    void OnDisable()
    {
        Stop();
        ReleaseRenderTexture();
    }

    /// <summary>
    /// 動画ファイルを設定してRenderTextureを準備する
    /// </summary>
    public void SetVideoFile(string fileName)
    {
        videoFileName = fileName;
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        videoPlayer.url = path;

        // AudioSourceモードに設定
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

        // RenderTextureを作成してVideoPlayerとRawImageに接続
        SetupRenderTexture(1280, 720);

        videoPlayer.Prepare();
    }

    /// <summary>
    /// RenderTextureを作成し、VideoPlayerとRawImageに接続する
    /// </summary>
    private void SetupRenderTexture(int width, int height)
    {
        ReleaseRenderTexture();

        renderTexture = new RenderTexture(width, height, 0);
        renderTexture.Create();

        videoPlayer.targetTexture = renderTexture;

        if (displayImage != null)
        {
            displayImage.texture = renderTexture;
        }
    }

    /// <summary>
    /// RenderTextureを解放する
    /// </summary>
    private void ReleaseRenderTexture()
    {
        if (renderTexture != null)
        {
            videoPlayer.targetTexture = null;
            if (displayImage != null)
                displayImage.texture = null;

            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = null;
        }
    }

    public void Play()
    {
        if (videoPlayer != null && !videoPlayer.isPlaying)
        {
            videoPlayer.Play();
            Debug.Log($"VideoController: 再生開始 - {videoFileName}");
        }
    }

    public void Pause()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
    }

    public void Stop()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
    }
}
