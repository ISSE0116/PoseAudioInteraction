using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// VideoPlayerを制御するクラス
/// 元動画の再生を担当
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class VideoController : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    [Header("Settings")]
    public bool playOnStart = false;
    public string videoFileName; // StreamingAssets内のファイル名

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        
        // AudioSourceモードに設定（必要に応じて）
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        
        // 動画パスの設定（StreamingAssetsの利用を想定）
        if (!string.IsNullOrEmpty(videoFileName))
        {
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
            videoPlayer.url = path;
        }

        videoPlayer.Prepare();

        if (playOnStart)
        {
            Play();
        }
    }

    public void Play()
    {
        if (!videoPlayer.isPlaying)
        {
            videoPlayer.Play();
        }
    }

    public void Pause()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
    }

    public void Stop()
    {
        videoPlayer.Stop();
    }

    public void SetVideoFile(string fileName)
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        videoPlayer.url = path;
        videoPlayer.Prepare();
    }
}
