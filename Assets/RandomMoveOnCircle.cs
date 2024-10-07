using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArcMovement : MonoBehaviour
{
    public Transform listener;
    public Transform soundSource;
    public Transform marker;
    public AudioSource audioSource;

    public float radius = 10f;
    public float rotationSpeed = 30f;

    private float startAngle;
    private float endAngle;
    private float currentAngle;
    private bool movingClockwise = true;
    private bool isMoving = false;
    private bool isRepeating = false;
    private bool isStaticMode = false;
    private bool isVerticalMode = false; // 垂直モードのフラグを追加

    public Button startButton;
    public Button nextButton;
    public Button repeatButton;
    public Button modeButton;
    public Button movementModeButton; // 水平・垂直切り替え用のボタンを追加
    public TMP_InputField fileNameInput;
    public Button saveButton;

    private float previousStartAngle;
    private float previousEndAngle;
    private bool previousMovingClockwise;

    private List<(float time, float angle)> trajectoryData = new List<(float, float)>();
    private float startTime;

    void Start()
    {
        startButton.onClick.AddListener(StartMovement);
        nextButton.onClick.AddListener(NextMovement);
        repeatButton.onClick.AddListener(RepeatMovement);
        modeButton.onClick.AddListener(ToggleMode);
        movementModeButton.onClick.AddListener(ToggleMovementMode); // 垂直モードの切り替えボタンにリスナーを追加
        saveButton.onClick.AddListener(SaveTrajectoryDataWithFileName);

        SetRandomAngles();
        UpdatePosition();

        fileNameInput.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isMoving && !isStaticMode)
        {
            // 現在の角度を更新 (角速度に基づいて、時計回りか反時計回りかを確認)
            if (movingClockwise)
            {
                currentAngle += rotationSpeed * Time.deltaTime;  // 時間に基づいて角度を徐々に増加
                if (currentAngle >= endAngle)
                {
                    currentAngle = endAngle; // 終了角度に到達したら固定
                    StopMovement(); // 動作を停止
                }
            }
            else
            {
                currentAngle -= rotationSpeed * Time.deltaTime;  // 反時計回りに角度を減少
                if (currentAngle <= endAngle)
                {
                    currentAngle = endAngle; // 終了角度に到達したら固定
                    StopMovement(); // 動作を停止
                }
            }

            // 音源の位置を更新
            UpdatePosition();
            marker.position = soundSource.position;  // マーカーの位置を音源と同期

            // 時間と角度を記録
            float elapsedTime = Time.time - startTime;
            trajectoryData.Add((elapsedTime, currentAngle));
        }
    }


    // 現在の角度に基づいて音源の位置を更新する
    void UpdatePosition()
    {
        float radians = currentAngle * Mathf.Deg2Rad;
        
        if (isVerticalMode)
        {
            // 垂直方向の動き: Z軸固定で、X-Y平面を動く
            float targetX = listener.position.x + radius * Mathf.Cos(radians);
            float targetY = listener.position.y + radius * Mathf.Sin(radians);

            // Z軸をリスナーの位置に固定する
            soundSource.position = new Vector3(targetX, targetY, listener.position.z);
        }
        else
        {
            // 水平方向の動き: Y軸固定で、X-Z平面を動く
            float targetX = listener.position.x + radius * Mathf.Cos(radians);
            float targetZ = listener.position.z + radius * Mathf.Sin(radians);

            // Y軸を固定して、X-Z平面を動く
            soundSource.position = new Vector3(targetX, soundSource.position.y, targetZ);
        }
    }


    // 移動を開始するメソッド
    void StartMovement()
    {
        if (!isStaticMode)
        {
            if (!isMoving)
            {
                SaveCurrentAngles();
                isMoving = true;
                currentAngle = startAngle;  // 角度を開始角度に設定
                UpdatePosition();
                marker.position = soundSource.position;

                // 軌跡データを初期化し、開始時刻を設定
                trajectoryData.Clear();
                startTime = Time.time;

                audioSource.Play(); // 音を鳴らす
                Debug.Log("Start movement at angle: " + startAngle);
            }
        }
        else
        {
            // 静的モードで5秒間音を鳴らす
            StartCoroutine(PlaySoundForDuration(5f));
        }
    }

    // 次の移動を設定するメソッド
    void NextMovement()
    {
        SetRandomAngles(); // ランダムに次の角度を設定
        currentAngle = startAngle;
        UpdatePosition();
        marker.position = soundSource.position;
    }

    // 前回の動きを繰り返すメソッド
    void RepeatMovement()
    {
        if (isStaticMode)
        {
            currentAngle = previousStartAngle;
            UpdatePosition();
            marker.position = soundSource.position;
            StartCoroutine(PlaySoundForDuration(5f));
        }
        else
        {
            startAngle = previousStartAngle;
            endAngle = previousEndAngle;
            movingClockwise = previousMovingClockwise;
            isMoving = true;
            currentAngle = startAngle;
            UpdatePosition();
            marker.position = soundSource.position;
            audioSource.Play();
        }
    }

    // モードを切り替えるメソッド（動く・静止の切り替え）
    void ToggleMode()
    {
        isStaticMode = !isStaticMode;
        Debug.Log("Mode: " + (isStaticMode ? "Static" : "Moving"));
        if (isStaticMode)
        {
            isMoving = false;
            audioSource.Stop();
        }
    }

    // 水平・垂直モードを切り替えるメソッド
    void ToggleMovementMode()
    {
        isVerticalMode = !isVerticalMode;
        if (isVerticalMode)
        {
            // 垂直モードでは、水平方向の0度から開始
            currentAngle = 0f;
            startAngle = 0f;  // 水平方向の0度を開始角度に設定
            Debug.Log("Switched to Vertical Mode. Start from horizontal 0 degrees.");
        }
        else
        {
            // 水平モードでは、0度の位置から開始
            currentAngle = 0f;
            startAngle = 0f;  // 水平方向の0度を開始角度に設定
            Debug.Log("Switched to Horizontal Mode. Start from 0 degrees.");
        }
        UpdatePosition();  // 切り替えた際の位置を更新
    }

    // 移動を停止するメソッド
    void StopMovement()
    {
        isMoving = false;
        audioSource.Stop();

        if (isRepeating)
        {
            StartMovement();
        }

        // 入力フィールドと保存ボタンを表示
        fileNameInput.gameObject.SetActive(true);
        saveButton.gameObject.SetActive(true);
    }

    // 現在の角度設定を保存するメソッド
    void SaveCurrentAngles()
    {
        previousStartAngle = startAngle;
        previousEndAngle = endAngle;
        previousMovingClockwise = movingClockwise;
    }

    // ランダムな開始角度と終了角度を設定するメソッド
    void SetRandomAngles()
    {
        startAngle = Random.Range(0, 12) * 30;
        movingClockwise = (Random.value > 0.5f);
        float[] angleOptions = { 60f, 90f, 120f, 150f, 180f, 210f, 240f, 270f, 300f, 330f, 360f};
        float angleDifference = angleOptions[Random.Range(0, angleOptions.Length)];
        endAngle = startAngle + (movingClockwise ? angleDifference : -angleDifference);

        if (endAngle < 0f) endAngle += 360f;
        if (endAngle >= 360f) endAngle -= 360f;

        Debug.Log($"Start Angle: {startAngle}, End Angle: {endAngle}, Clockwise: {movingClockwise}, Angle Difference: {angleDifference}");
    }

    // 指定した秒数だけ音を鳴らすコルーチン
    private IEnumerator PlaySoundForDuration(float duration)
    {
        audioSource.Play();
        yield return new WaitForSeconds(duration);
        audioSource.Stop();
    }

    // 入力されたファイル名で軌跡データをCSVとして保存
    private void SaveTrajectoryDataWithFileName()
    {
        string fileName = fileNameInput.text;
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogWarning("File name is empty. Please enter a file name.");
            return;
        }

        string path = $"{fileName}.csv";
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("Time,Angle");
            foreach (var data in trajectoryData)
            {
                writer.WriteLine($"{data.time},{data.angle}");
            }
        }
        Debug.Log("Trajectory data saved to " + path);

        // 入力フィールドと保存ボタンを非表示に
        fileNameInput.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(false);
    }
}
