using UnityEngine;

public class DynamicAudioControl : MonoBehaviour
{
    public Transform listenerTransform; // リスナー（通常はカメラ）のTransform
    public AudioSource audioSource;     // 音源のAudio Source
    public Transform markerTransform;   // スフィアのTransform（可視化用）

    void Update()
    {
        // リスナーと音源の距離を計算
        float distance = Vector3.Distance(listenerTransform.position, audioSource.transform.position);

        // 距離に基づいた音量調整（距離が大きいほど音量は小さくなる）
        audioSource.volume = Mathf.Clamp(1.0f - (distance / 20.0f), 0.0f, 1.0f);

        // 音源の位置を動的に変更する例（左右に動かす）
        float newX = Mathf.PingPong(Time.time * 2, 10); // スピードを変更するためにTime.timeに2を掛ける
        audioSource.transform.position = new Vector3(newX, 0, 0);

        // マーカーの位置も音源に合わせて更新
        if (markerTransform != null)
        {
            markerTransform.position = audioSource.transform.position;
        }
    }
}