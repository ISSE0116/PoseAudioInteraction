using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 円周上での音源移動を制御する統合スクリプト
/// 移動モード：連続移動 / 45度刻み離散移動
/// 方向モード：水平 / 垂直
/// </summary>
public class CircleMovement : MonoBehaviour
{
    // ========== 移動モード ==========
    public enum MovementType
    {
        Continuous,  // 連続移動（旧CircleMoveContinuous, MoveOnCircle3）
        Discrete     // 45度刻み（旧MoveOnCircle_2）
    }

    [Header("Movement Settings")]
    public MovementType movementType = MovementType.Continuous;
    public bool isVerticalMode = false;   // true: 垂直, false: 水平

    // ========== References ==========
    
    [Header("Targets")]
    public Transform listener;
    public Transform soundSource;
    public Transform marker;
    public AudioSource audioSource;

    [Header("Motion")]
    public float radius = 10f;
    public float rotationSpeed = 30f;      // 連続モード用
    public float rotationInterval = 1f;    // 離散モード用（秒）
    public int totalRevolutions = 3;       // 周回数

    // ========== UI ==========
    
    [Header("Buttons")]
    public Button startButton;
    public Button nextButton;
    public Button repeatButton;
    public Button modeButton;           // 静止/移動切替
    public Button movementModeButton;   // 水平/垂直切替
    public Button saveButton;

    [Header("Input")]
    public TMP_InputField fileNameInput;

    // ========== 内部変数 ==========
    
    private float startAngle;
    private float currentAngle;
    private bool movingClockwise = true;
    private bool isMoving = false;
    private bool isStaticMode = false;
    private int currentRevolutions = 0;

    // 離散モード用
    private float rotationTimer = 0f;
    private bool reachedBoundary = false;
    private int verticalRepeats = 0;

    // 前回の設定保存
    private float previousStartAngle;
    private bool previousMovingClockwise;

    // 軌跡データ
    private readonly List<(float time, float angle)> trajectoryData = new();
    private float startTime;
    private float dataInterval = 0.1f;
    private float nextDataTime = 0f;

    // ========== ライフサイクル ==========

    void Start()
    {
        SetupButtons();
        SetRandomAngles();
        UpdatePosition();
        
        fileNameInput?.gameObject.SetActive(false);
        saveButton?.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isMoving || isStaticMode) return;

        if (movementType == MovementType.Continuous)
        {
            UpdateContinuousMovement();
        }
        else
        {
            UpdateDiscreteMovement();
        }
    }

    // ========== 移動更新 ==========

    private void UpdateContinuousMovement()
    {
        // 角度を更新
        currentAngle += (movingClockwise ? 1 : -1) * rotationSpeed * Time.deltaTime;

        // 周回カウント
        if (currentAngle >= 360f)
        {
            currentRevolutions++;
            currentAngle -= 360f;
        }
        else if (currentAngle < 0f)
        {
            currentRevolutions++;
            currentAngle += 360f;
        }

        UpdatePosition();
        RecordTrajectory();
        CheckCompletion();
    }

    private void UpdateDiscreteMovement()
    {
        rotationTimer += Time.deltaTime;
        if (rotationTimer < rotationInterval) return;

        rotationTimer = 0f;

        if (isVerticalMode)
        {
            MoveVerticalDiscrete();
        }
        else
        {
            MoveHorizontalDiscrete();
        }

        RecordTrajectory();
    }

    private void MoveHorizontalDiscrete()
    {
        currentAngle += movingClockwise ? 45f : -45f;

        if (currentAngle >= 360f)
        {
            currentAngle -= 360f;
            currentRevolutions++;
        }
        else if (currentAngle < 0f)
        {
            currentAngle += 360f;
            currentRevolutions++;
        }

        UpdatePosition();
        CheckCompletion();
    }

    private void MoveVerticalDiscrete()
    {
        currentAngle += movingClockwise ? 45f : -45f;

        if (movingClockwise && currentAngle >= 180f && !reachedBoundary)
        {
            currentAngle = 180f;
            reachedBoundary = true;
        }
        else if (movingClockwise && reachedBoundary)
        {
            currentAngle = 0f;
            reachedBoundary = false;
            verticalRepeats++;
        }
        else if (!movingClockwise && currentAngle <= 0f && !reachedBoundary)
        {
            currentAngle = 0f;
            reachedBoundary = true;
        }
        else if (!movingClockwise && reachedBoundary)
        {
            currentAngle = 180f;
            reachedBoundary = false;
            verticalRepeats++;
        }

        UpdatePosition();

        if (verticalRepeats >= totalRevolutions)
        {
            StopMovement();
        }
    }

    // ========== 位置更新 ==========

    private void UpdatePosition()
    {
        if (soundSource == null || listener == null) return;

        float radians = currentAngle * Mathf.Deg2Rad;
        Vector3 pos;

        if (isVerticalMode)
        {
            // 垂直方向：X-Y平面
            float x = listener.position.x + radius * Mathf.Cos(radians);
            float y = listener.position.y + radius * Mathf.Sin(radians);
            pos = new Vector3(x, y, listener.position.z);
        }
        else
        {
            // 水平方向：X-Z平面
            float x = listener.position.x + radius * Mathf.Cos(radians);
            float z = listener.position.z + radius * Mathf.Sin(radians);
            pos = new Vector3(x, soundSource.position.y, z);
        }

        soundSource.position = pos;
        
        // リスナー方向を向く
        Vector3 dir = (listener.position - pos).normalized;
        if (dir != Vector3.zero)
            soundSource.forward = dir;

        if (marker != null)
            marker.position = pos;
    }

    // ========== 制御メソッド ==========

    public void StartMovement()
    {
        if (isStaticMode)
        {
            StartCoroutine(PlaySoundForDuration(5f));
            return;
        }

        if (isMoving) return;

        previousStartAngle = startAngle;
        previousMovingClockwise = movingClockwise;
        
        isMoving = true;
        currentRevolutions = 0;
        verticalRepeats = 0;
        reachedBoundary = false;
        currentAngle = startAngle;
        rotationTimer = 0f;
        
        trajectoryData.Clear();
        startTime = Time.time;
        nextDataTime = 0f;

        UpdatePosition();
        audioSource?.Play();
        
        Debug.Log($"CircleMovement: 再生開始 (角度={startAngle}, 方向={movingClockwise})");
    }

    public void StopMovement()
    {
        isMoving = false;
        audioSource?.Stop();

        fileNameInput?.gameObject.SetActive(true);
        saveButton?.gameObject.SetActive(true);
        
        Debug.Log("CircleMovement: 再生停止");
    }

    public void NextMovement()
    {
        SetRandomAngles();
        currentAngle = startAngle;
        currentRevolutions = 0;
        verticalRepeats = 0;
        reachedBoundary = false;
        UpdatePosition();
    }

    public void RepeatMovement()
    {
        startAngle = previousStartAngle;
        movingClockwise = previousMovingClockwise;
        currentAngle = startAngle;
        currentRevolutions = 0;
        verticalRepeats = 0;
        reachedBoundary = false;

        if (isStaticMode)
        {
            UpdatePosition();
            StartCoroutine(PlaySoundForDuration(5f));
        }
        else
        {
            isMoving = true;
            UpdatePosition();
            audioSource?.Play();
        }
    }

    public void ToggleMode()
    {
        isStaticMode = !isStaticMode;
        if (isStaticMode)
        {
            isMoving = false;
            audioSource?.Stop();
        }
        Debug.Log($"CircleMovement: モード={isStaticMode}");
    }

    public void ToggleMovementMode()
    {
        isVerticalMode = !isVerticalMode;
        currentAngle = 0f;
        startAngle = 0f;
        UpdatePosition();
        Debug.Log($"CircleMovement: 方向={isVerticalMode}");
    }

    // ========== ヘルパー ==========

    private void SetupButtons()
    {
        startButton?.onClick.AddListener(StartMovement);
        nextButton?.onClick.AddListener(NextMovement);
        repeatButton?.onClick.AddListener(RepeatMovement);
        modeButton?.onClick.AddListener(ToggleMode);
        movementModeButton?.onClick.AddListener(ToggleMovementMode);
        saveButton?.onClick.AddListener(SaveTrajectoryData);
    }

    private void SetRandomAngles()
    {
        if (movementType == MovementType.Discrete)
        {
            int[] angles = { 0, 45, 90, 135, 180, 225, 270, 315 };
            startAngle = angles[Random.Range(0, angles.Length)];
        }
        else
        {
            startAngle = Random.Range(0, 24) * 15; // 15度刻み
        }
        
        currentAngle = startAngle;
        movingClockwise = Random.value > 0.5f;
        previousStartAngle = startAngle;
        previousMovingClockwise = movingClockwise;
    }

    private void RecordTrajectory()
    {
        float elapsed = Time.time - startTime;
        if (elapsed >= nextDataTime)
        {
            trajectoryData.Add((elapsed, currentAngle));
            nextDataTime += dataInterval;
        }
    }

    private void CheckCompletion()
    {
        if (currentRevolutions >= totalRevolutions)
        {
            StopMovement();
            Debug.Log($"CircleMovement: {totalRevolutions}周完了");
        }
    }

    private IEnumerator PlaySoundForDuration(float duration)
    {
        audioSource?.Play();
        yield return new WaitForSeconds(duration);
        audioSource?.Stop();
    }

    private void SaveTrajectoryData()
    {
        if (fileNameInput == null) return;
        
        string fileName = fileNameInput.text;
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogWarning("CircleMovement: ファイル名が空です");
            return;
        }

        string path = $"{fileName}.csv";
        using (var writer = new StreamWriter(path))
        {
            writer.WriteLine("Time,Angle");
            foreach (var data in trajectoryData)
            {
                writer.WriteLine($"{data.time},{data.angle}");
            }
        }

        Debug.Log($"CircleMovement: 軌跡データ保存 -> {path}");
        fileNameInput.gameObject.SetActive(false);
        saveButton?.gameObject.SetActive(false);
    }
}
