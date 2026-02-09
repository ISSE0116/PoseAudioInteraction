using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoveOnCircle_2 : MonoBehaviour
{
    public Transform listener;
    public Transform audioSourceMarker;
    public Transform marker;
    public AudioSource audioSource;

    public float radius = 10f;
    public float rotationInterval = 1f; // 1秒ごとに45度移動
    private float rotationTimer = 0f;

    private float startAngle;
    private float currentAngle;
    private bool movingClockwise = true;
    private bool isMoving = false;
    private bool isStaticMode = false;
    private bool isVerticalMode = false;
    private int currentRevolutions = 0;
    private int totalRevolutions = 3;
    private int verticalRepeats = 0;
    private int totalRepeats = 3;

    public Button startButton;
    public Button nextButton;
    public Button repeatButton;
    public Button modeButton;
    public Button movementModeButton;
    public TMP_InputField fileNameInput;
    public Button saveButton;

    // 新しく追加するボタン
    public Button frontBackButton;
    public Button diagonalFrontBackButton;
    public Button diagonalFrontButton;
    public Button diagonalBackButton;
    public Button selectAngleButton;
    public Button selectRotationButton;

    private List<(float time, float angle)> trajectoryData = new List<(float, float)>();
    private float startTime;
    private float dataInterval = 0.1f;
    private float nextDataTime = 0f;
    private bool reachedBoundary = false;

    private float previousStartAngle;
    private bool previousMovingClockwise;
    private float selectedAngle = 0f;
    private float currentSelectedAngle = 0f;

    void Start()
    {
        startButton.onClick.AddListener(StartMovement);
        nextButton.onClick.AddListener(NextMovement);
        repeatButton.onClick.AddListener(RepeatMovement);
        modeButton.onClick.AddListener(ToggleMode);
        movementModeButton.onClick.AddListener(ToggleMovementMode);
        saveButton.onClick.AddListener(SaveTrajectoryDataWithFileName);

        // 新しく追加したボタンのイベントリスナーを設定
        frontBackButton.onClick.AddListener(MoveToFrontBack);
        diagonalFrontBackButton.onClick.AddListener(MoveToDiagonalFrontBack);
        diagonalFrontButton.onClick.AddListener(MoveToDiagonalFront);
        diagonalBackButton.onClick.AddListener(MoveToDiagonalBack);
        selectAngleButton.onClick.AddListener(SetSelectedAngle);
        selectRotationButton.onClick.AddListener(SetRotationDirection);

        SetRandomAngles();
        UpdatePosition();

        fileNameInput.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isMoving && !isStaticMode)
        {
            rotationTimer += Time.deltaTime;
            if (rotationTimer >= rotationInterval)
            {
                rotationTimer = 0f; // タイマーをリセット
                if (isVerticalMode)
                {
                    MoveVertical();
                }
                else
                {
                    MoveHorizontal();
                }

                float elapsedTime = Time.time - startTime;
                if (elapsedTime >= nextDataTime)
                {
                    trajectoryData.Add((elapsedTime, currentAngle));
                    nextDataTime += dataInterval;
                }
            }
        }
    }

    void MoveHorizontal()
    {
        if (movingClockwise)
        {
            currentAngle += 45f;
            if (currentAngle >= 360f) 
            {
                currentAngle -= 360f;
                currentRevolutions++;
                Debug.Log("Completed one revolution. Current revolutions: " + currentRevolutions);
            }
        }
        else
        {
            currentAngle -= 45f;
            if (currentAngle < 0f)
            {
                currentAngle += 360f;
                currentRevolutions++;
                Debug.Log("Completed one revolution. Current revolutions: " + currentRevolutions);
            }
        }

        if (currentRevolutions >= totalRevolutions)
        {
            StopMovement();
            Debug.Log("Completed total revolutions: " + totalRevolutions);
        }

        UpdatePosition();
    }

    void MoveVertical()
    {
        if (movingClockwise)
        {
            currentAngle += 45f;
            if (currentAngle >= 180f && !reachedBoundary)
            {
                currentAngle = 180f;
                reachedBoundary = true;
            }
            else if (reachedBoundary)
            {
                currentAngle = 0f;
                reachedBoundary = false;
                verticalRepeats++;
                Debug.Log("Vertical movement repetition: " + verticalRepeats);
            }
        }
        else
        {
            currentAngle -= 45f;
            if (currentAngle <= 0f && !reachedBoundary)
            {
                currentAngle = 0f;
                reachedBoundary = true;
            }
            else if (reachedBoundary)
            {
                currentAngle = 180f;
                reachedBoundary = false;
                verticalRepeats++;
                Debug.Log("Vertical movement repetition: " + verticalRepeats);
            }
        }

        if (verticalRepeats >= totalRepeats)
        {
            StopMovement();
            Debug.Log("Completed total vertical repetitions: " + totalRepeats);
        }

        UpdatePosition();
    }

    void SetRandomAngles()
    {
        int[] angles = { 0, 45, 90, 135, 180, 225, 270, 315 };
        startAngle = angles[Random.Range(0, angles.Length)];
        currentAngle = startAngle;

        movingClockwise = (Random.value > 0.5f);
        previousStartAngle = startAngle;
        previousMovingClockwise = movingClockwise;
        Debug.Log($"Start Angle: {startAngle}, Clockwise: {movingClockwise}");
    }

    void UpdatePosition()
        {
            float radians = currentAngle * Mathf.Deg2Rad;

            if (isVerticalMode)
            {
                float targetX = listener.position.x + radius * Mathf.Cos(radians);
                float targetY = listener.position.y + 1.5f + Mathf.Abs(radius * Mathf.Sin(radians));
                audioSourceMarker.position = new Vector3(targetX, targetY, listener.position.z);
            }
            else
            {
                float targetX = listener.position.x + radius * Mathf.Cos(radians);
                float targetZ = listener.position.z + radius * Mathf.Sin(radians);
                audioSourceMarker.position = new Vector3(targetX, audioSourceMarker.position.y, targetZ);
            }

            marker.position = audioSourceMarker.position;

            // リスナーの方向に音源を向ける
            Vector3 directionToListener = (listener.position - audioSourceMarker.position).normalized;
            audioSourceMarker.forward = directionToListener;
        }


    void StartMovement()
        {
            if (!isStaticMode)
            {
                if (!isMoving)
                {
                    isMoving = true;
                    currentRevolutions = 0;
                    verticalRepeats = 0;
                    currentAngle = startAngle;
                    UpdatePosition();

                    trajectoryData.Clear();
                    startTime = Time.time;
                    nextDataTime = 0f;

                    audioSource.Play();
                    Debug.Log("Start movement at angle: " + startAngle);
                }
            }
            else
            {
                StartCoroutine(PlaySoundForDuration(5f));
            }
        }

    void StopMovement()
    {
        isMoving = false;
        audioSource.Stop();

        fileNameInput.gameObject.SetActive(true);
        saveButton.gameObject.SetActive(true);
    }

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

    void ToggleMovementMode()
    {
        isVerticalMode = !isVerticalMode;
        currentAngle = 0f;
        startAngle = 0f;
        UpdatePosition();
    }

    void NextMovement()
    {
        SetRandomAngles();
        currentAngle = startAngle;
        currentRevolutions = 0;
        verticalRepeats = 0;
        reachedBoundary = false;
        UpdatePosition();
        Debug.Log("Next movement started.");
    }

    void RepeatMovement()
    {
        startAngle = previousStartAngle;
        movingClockwise = previousMovingClockwise;
        currentAngle = startAngle;
        currentRevolutions = 0;
        verticalRepeats = 0;
        reachedBoundary = false;
        isMoving = true;

        UpdatePosition();
        audioSource.Play();
        Debug.Log("Repeat movement started with angle: " + startAngle);
    }

    void MoveToFrontBack()
    {
        StartCoroutine(MoveBetweenAngles(new float[] { 90f, 270f }, 2f, 3));
    }

    void MoveToDiagonalFrontBack()
    {
        StartCoroutine(MoveBetweenAngles(new float[] { 45f, 315f }, 2f, 3));
    }

    void MoveToDiagonalFront()
    {
        StartCoroutine(MoveBetweenAngles(new float[] { 45f, 135f }, 2f, 3));
    }

    void MoveToDiagonalBack()
    {
        StartCoroutine(MoveBetweenAngles(new float[] { 225f, 315f }, 2f, 3));
    }

    private IEnumerator PlaySoundForDuration(float duration)
    {
        audioSource.Play();
        yield return new WaitForSeconds(duration);
        audioSource.Stop();
    }

    private IEnumerator MoveBetweenAngles(float[] angles, float duration, int repeatCount)
    {
        for (int i = 0; i < repeatCount; i++)
        {
            foreach (float angle in angles)
            {
                currentAngle = angle;
                UpdatePosition();
                audioSource.Play();
                yield return new WaitForSeconds(duration);
                audioSource.Stop();
            }
        }
    }


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

        fileNameInput.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(false);
    }

    void SetSelectedAngle()
    {
        currentSelectedAngle += 45f;
        if (currentSelectedAngle >= 360f)
        {
            currentSelectedAngle = 0f;
        }

        selectedAngle = currentSelectedAngle;
        Debug.Log("Selected Start Angle: " + selectedAngle);
    }

    // SelectRotationButtonで呼び出されるメソッド
    void SetRotationDirection()
    {
        movingClockwise = !movingClockwise;
        Debug.Log("Selected Rotation Direction: " + (movingClockwise ? "Clockwise" : "Counterclockwise"));
    }

}