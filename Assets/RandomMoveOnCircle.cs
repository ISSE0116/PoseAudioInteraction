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

    public Button startButton;
    public Button nextButton;
    public Button repeatButton;
    public Button modeButton;
    public TMP_InputField fileNameInput; // ファイル名入力用のTMP_InputField
    public Button saveButton; // データ保存用のボタン

    private float previousStartAngle;
    private float previousEndAngle;
    private bool previousMovingClockwise;

    private List<(float time, float angle)> trajectoryData = new List<(float, float)>();
    private float startTime;

    void Start()
    {
        // ボタンのクリックイベントを設定
        startButton.onClick.AddListener(StartMovement);
        nextButton.onClick.AddListener(NextMovement);
        repeatButton.onClick.AddListener(RepeatMovement);
        modeButton.onClick.AddListener(ToggleMode);
        saveButton.onClick.AddListener(SaveTrajectoryDataWithFileName); // 保存ボタンのクリックイベント

        // 音源の最初の位置を設定
        SetRandomAngles();
        UpdatePosition();

        // InputFieldと保存ボタンを非表示にする
        fileNameInput.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isMoving && !isStaticMode)
        {
            // 現在の角度を更新（時計回りか反時計回りかを確認）
            if (movingClockwise)
            {
                currentAngle += rotationSpeed * Time.deltaTime;
                if (currentAngle >= endAngle)
                {
                    currentAngle = endAngle;
                    StopMovement();
                }
            }
            else
            {
                currentAngle -= rotationSpeed * Time.deltaTime;
                if (currentAngle <= endAngle)
                {
                    currentAngle = endAngle;
                    StopMovement();
                }
            }

            // 音源とマーカーの位置を更新
            UpdatePosition();
            marker.position = soundSource.position;

            // 軌跡データの記録
            float elapsedTime = Time.time - startTime;
            trajectoryData.Add((elapsedTime, currentAngle));
        }
    }

    // 現在の角度に基づいて音源の位置を更新する
    void UpdatePosition()
    {
        float radians = currentAngle * Mathf.Deg2Rad;
        float targetX = listener.position.x + radius * Mathf.Cos(radians);
        float targetZ = listener.position.z + radius * Mathf.Sin(radians);
        soundSource.position = new Vector3(targetX, soundSource.position.y, targetZ);
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
                currentAngle = startAngle;
                UpdatePosition();
                marker.position = soundSource.position;

                // 軌跡データを初期化し、開始時刻を設定
                trajectoryData.Clear();
                startTime = Time.time;

                audioSource.Play(); // 音を鳴らす
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
        SetRandomAngles();
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
        startAngle = Random.Range(0, 24) * 15;
        movingClockwise = (Random.value > 0.5f);
        float[] angleOptions = { 45f, 60f, 75f, 90f };
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