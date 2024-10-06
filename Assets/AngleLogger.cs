using System;
using System.IO;
using UnityEngine;

public class AngleLogger : MonoBehaviour
{
    public Transform listener; // リスナー (通常はカメラなど)
    public Transform soundSource; // 音源
    
    private string filePath;
    private StreamWriter writer;
    private float startTime;

    void Start()
    {
        // CSVファイルのパスを指定
        filePath = Application.dataPath + "/angle_log.csv";
        
        // ファイルを作成、列名を書き込む
        writer = new StreamWriter(filePath);
        writer.WriteLine("elapsed_time, angle");
        writer.Flush();

        // 開始時間を取得
        startTime = Time.time;
    }

    void Update()
    {
        // リスナーと音源の位置を取得
        Vector3 listenerPosition = listener.position;
        Vector3 soundSourcePosition = soundSource.position;
        
        // 2点間のベクトルを計算
        Vector3 direction = soundSourcePosition - listenerPosition;
        
        // XZ平面上での角度を計算
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
        
        // 角度を0〜360度に変換（リスナーの真右を90度として調整）
        angle = (angle + 90) % 360;

        // 角度が負の場合は360度を足して0〜360度に変換
        if (angle < 0)
        {
            angle += 360;
        }

        // 経過時間（開始時間からの相対時間）
        float elapsedTime = Time.time - startTime;
        
        // データをCSVに書き込む（時間と角度のみ）
        string logEntry = string.Format("{0},{1}",
            elapsedTime.ToString("F2"),  // 経過時間を少数2桁で表示
            angle.ToString("F2"));       // 角度を少数2桁で表示
        
        writer.WriteLine(logEntry);
        writer.Flush(); // 即時書き込み
    }

    void OnApplicationQuit()
    {
        // アプリ終了時にファイルを閉じる
        writer.Close();
    }
}