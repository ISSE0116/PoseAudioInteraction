using UnityEngine;

public class StaticSoundListener : MonoBehaviour
{
    public Transform listenerTransform; // リスナー（通常はカメラ）のTransform
    public float rotationSpeed = 20.0f; // 回転速度
    public float radius = 5.0f;         //  リスナーからの距離（半径）
    public float start;                 //音源の開始場所
    public float finish;                //音源の終了場所

    public Transform markerTransform;   // スフィアのTransform（可視化用）
    public AudioSource audioSource;     // 音源のAudio Source

    private float angle = 0.0f; // 現在の角度

    void Update()
    {
        // 角度を更新（時間に基づいて回転速度を考慮）
        angle += rotationSpeed * Time.deltaTime;

        // 角度を360度範囲内に維持
        if (angle >= 360.0f) 
            angle -= 360.0f; 

        // 音源の新しい位置を計算（リスナーを中心とした円運動）
        
        float x = listenerTransform.position.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        float z = listenerTransform.position.z + radius * Mathf.Sin(angle * Mathf.Deg2Rad);

        if(angle>=finish){
            transform.position = new Vector3(x, transform.position.y, z);
        }

        // 新しい位置を音源に設定
        transform.position = new Vector3(x, transform.position.y, z);

        // デバッグログで位置を確認
        //Debug.Log($"New Position: x={x}, z={z}, angle={angle}");

        // マーカーの位置を音源に合わせて更新
        if (markerTransform != null)
        {
            markerTransform.position = transform.position;
        }
    }
}