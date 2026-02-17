using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PoseEstimatorのランドマーク座標をWebカメラ映像上にオーバーレイ描画するクラス
/// RawImageの上に透明なテクスチャで骨格（点と線）を描画する
/// </summary>
public class PoseOverlayRenderer : MonoBehaviour
{
    [Header("References")]
    public PoseEstimator poseEstimator;
    public RawImage overlayImage;

    [Header("Rendering Settings")]
    public Color jointColor = new Color(1f, 0f, 0f, 1f);      // 赤
    public Color boneColor = new Color(1f, 1f, 0f, 1f);        // 黄
    [Range(2, 10)]
    public int jointRadius = 5;
    [Range(1, 5)]
    public int boneWidth = 2;

    private Texture2D _overlayTexture;
    private Color32[] _clearPixels;
    private int _texWidth;
    private int _texHeight;

    // 骨格の接続定義（Mediapipe Pose Landmark のペア）
    private static readonly int[][] BoneConnections = new int[][]
    {
        // 顔
        new[] {0, 1}, new[] {1, 2}, new[] {2, 3}, new[] {3, 7},   // 右目
        new[] {0, 4}, new[] {4, 5}, new[] {5, 6}, new[] {6, 8},   // 左目
        new[] {9, 10},                                               // 口

        // 体幹
        new[] {11, 12},                                              // 肩
        new[] {11, 23}, new[] {12, 24},                              // 胴体
        new[] {23, 24},                                              // 腰

        // 右腕
        new[] {12, 14}, new[] {14, 16},                              // 右肩→右肘→右手首
        new[] {16, 18}, new[] {16, 20}, new[] {16, 22}, new[] {18, 20}, // 右手

        // 左腕
        new[] {11, 13}, new[] {13, 15},                              // 左肩→左肘→左手首
        new[] {15, 17}, new[] {15, 19}, new[] {15, 21}, new[] {17, 19}, // 左手

        // 右脚
        new[] {24, 26}, new[] {26, 28},                              // 右腰→右膝→右足首
        new[] {28, 30}, new[] {28, 32}, new[] {30, 32},              // 右足

        // 左脚
        new[] {23, 25}, new[] {25, 27},                              // 左腰→左膝→左足首
        new[] {27, 29}, new[] {27, 31}, new[] {29, 31},              // 左足
    };

    void Update()
    {
        if (poseEstimator == null || overlayImage == null)
            return;

        if (!poseEstimator.IsDetected)
        {
            ClearOverlay();
            return;
        }

        EnsureTexture();
        ClearTexture();
        DrawSkeleton();
        _overlayTexture.Apply();
    }

    private void EnsureTexture()
    {
        // WebCamTextureに合わせたサイズでテクスチャを作成
        var webCam = poseEstimator.webCamDisplay?.GetWebCamTexture();
        int targetWidth = webCam != null ? webCam.width : 640;
        int targetHeight = webCam != null ? webCam.height : 480;

        if (_overlayTexture == null || _texWidth != targetWidth || _texHeight != targetHeight)
        {
            if (_overlayTexture != null)
                Destroy(_overlayTexture);

            _texWidth = targetWidth;
            _texHeight = targetHeight;
            _overlayTexture = new Texture2D(_texWidth, _texHeight, TextureFormat.RGBA32, false);
            _overlayTexture.filterMode = FilterMode.Bilinear;

            // クリア用ピクセル配列を事前作成
            _clearPixels = new Color32[_texWidth * _texHeight];
            var transparent = new Color32(0, 0, 0, 0);
            for (int i = 0; i < _clearPixels.Length; i++)
                _clearPixels[i] = transparent;

            overlayImage.texture = _overlayTexture;
        }
    }

    private void ClearTexture()
    {
        _overlayTexture.SetPixels32(_clearPixels);
    }

    private void ClearOverlay()
    {
        if (_overlayTexture != null)
        {
            ClearTexture();
            _overlayTexture.Apply();
        }
    }

    private void DrawSkeleton()
    {
        var landmarks = poseEstimator.LatestLandmarks;
        if (landmarks == null || landmarks.Count < 33)
            return;

        // ボーン（線）を描画
        Color32 bone32 = boneColor;
        foreach (var conn in BoneConnections)
        {
            var lm1 = landmarks[conn[0]];
            var lm2 = landmarks[conn[1]];

            int x1 = (int)(lm1.x * _texWidth);
            int y1 = (int)(lm1.y * _texHeight);
            int x2 = (int)(lm2.x * _texWidth);
            int y2 = (int)(lm2.y * _texHeight);

            DrawLine(x1, y1, x2, y2, bone32);
        }

        // ジョイント（点）を描画
        Color32 joint32 = jointColor;
        for (int i = 0; i < 33; i++)
        {
            var lm = landmarks[i];
            int x = (int)(lm.x * _texWidth);
            int y = (int)(lm.y * _texHeight);

            DrawCircle(x, y, jointRadius, joint32);
        }
    }

    // ========== 描画ヘルパー ==========

    private void DrawCircle(int cx, int cy, int radius, Color32 color)
    {
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                if (dx * dx + dy * dy <= radius * radius)
                {
                    int px = cx + dx;
                    int py = cy + dy;
                    if (px >= 0 && px < _texWidth && py >= 0 && py < _texHeight)
                    {
                        _overlayTexture.SetPixel(px, py, color);
                    }
                }
            }
        }
    }

    private void DrawLine(int x0, int y0, int x1, int y1, Color32 color)
    {
        // Bresenhamのライン描画アルゴリズム（太さ対応）
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            // 太さ分の描画
            for (int w = -boneWidth / 2; w <= boneWidth / 2; w++)
            {
                int px = x0 + (dy > dx ? w : 0);
                int py = y0 + (dy > dx ? 0 : w);
                if (px >= 0 && px < _texWidth && py >= 0 && py < _texHeight)
                {
                    _overlayTexture.SetPixel(px, py, color);
                }
            }

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    void OnDestroy()
    {
        if (_overlayTexture != null)
        {
            Destroy(_overlayTexture);
            _overlayTexture = null;
        }
    }
}
