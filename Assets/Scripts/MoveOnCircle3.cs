using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoveOnCircle3 : MonoBehaviour
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
    private bool isVerticalMode = false;
    private int currentRevolutions = 0;
    private int totalRevolutions = 3;

    public Button startButton;
    public Button nextButton;
    public Button repeatButton;
    public Button modeButton;
    public Button movementModeButton;
    public TMP_InputField fileNameInput;
    public Button saveButton;

    private float previousStartAngle;
    private float previousEndAngle;
    private bool previousMovingClockwise;

    private List<(float time, float angle)> trajectoryData = new List<(float, float)>();
    private float startTime;

    private float dataInterval = 0.1f;
    private float nextDataTime = 0f;

    void Start()
    {
        startButton.onClick.AddListener(StartMovement);
        nextButton.onClick.AddListener(NextMovement);
        repeatButton.onClick.AddListener(RepeatMovement);
        modeButton.onClick.AddListener(ToggleMode);
        movementModeButton.onClick.AddListener(ToggleMovementMode);
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
            if (movingClockwise)
            {
                currentAngle += rotationSpeed * Time.deltaTime;
            }
            else
            {
                currentAngle -= rotationSpeed * Time.deltaTime;
            }

            if (currentAngle >= 360f)
            {
                currentRevolutions++;
                currentAngle -= 360f;
                Debug.Log("Completed one revolution. Current revolutions: " + currentRevolutions);
            }
            else if (currentAngle < 0f)
            {
                currentRevolutions++;
                currentAngle += 360f;
                Debug.Log("Completed one revolution. Current revolutions: " + currentRevolutions);
            }

            UpdatePosition();
            marker.position = soundSource.position;

            float elapsedTime = Time.time - startTime;
            if (elapsedTime >= nextDataTime)
            {
                trajectoryData.Add((elapsedTime, currentAngle));
                nextDataTime += dataInterval;
            }

            if (currentRevolutions >= totalRevolutions)
            {
                StopMovement();
                Debug.Log("Completed total revolutions: " + totalRevolutions);
            }
        }
    }

    void UpdatePosition()
    {
        float radians = currentAngle * Mathf.Deg2Rad;

        if (isVerticalMode)
        {
            float targetX = listener.position.x + radius * Mathf.Cos(radians);
            float targetY = listener.position.y + radius * Mathf.Sin(radians);
            soundSource.position = new Vector3(targetX, targetY, listener.position.z);
        }
        else
        {
            float targetX = listener.position.x + radius * Mathf.Cos(radians);
            float targetZ = listener.position.z + radius * Mathf.Sin(radians);
            soundSource.position = new Vector3(targetX, soundSource.position.y, targetZ);
        }

        // リスナーの方向を常に向くように音源を回転（Steam Audio 用）
        Vector3 directionToListener = (listener.position - soundSource.position).normalized;
        if (directionToListener != Vector3.zero)
        {
            soundSource.forward = directionToListener;
        }
    }

    void StartMovement()
    {
        if (!isStaticMode)
        {
            if (!isMoving)
            {
                SaveCurrentAngles();
                isMoving = true;
                currentRevolutions = 0;
                currentAngle = startAngle;
                UpdatePosition();
                marker.position = soundSource.position;

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

    void NextMovement()
    {
        SetRandomAngles();
        currentAngle = startAngle;
        currentRevolutions = 0;
        UpdatePosition();
        marker.position = soundSource.position;
    }

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
            currentRevolutions = 0;
            currentAngle = startAngle;
            UpdatePosition();
            marker.position = soundSource.position;
            audioSource.Play();
        }
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
        Debug.Log(isVerticalMode ? "Switched to Vertical Mode" : "Switched to Horizontal Mode");
        UpdatePosition();
    }

    void StopMovement()
    {
        isMoving = false;
        audioSource.Stop();

        if (isRepeating)
        {
            StartMovement();
        }

        fileNameInput.gameObject.SetActive(true);
        saveButton.gameObject.SetActive(true);
    }

    void SaveCurrentAngles()
    {
        previousStartAngle = startAngle;
        previousEndAngle = endAngle;
        previousMovingClockwise = movingClockwise;
    }

    void SetRandomAngles()
    {
        startAngle = Random.Range(0, 24) * 15;
        movingClockwise = (Random.value > 0.5f);

        float totalAngle = 360f * 3;
        endAngle = startAngle + (movingClockwise ? totalAngle : -totalAngle);
        endAngle %= 360f;

        Debug.Log($"Start Angle: {startAngle}, End Angle: {endAngle}, Clockwise: {movingClockwise}");
    }

    private IEnumerator PlaySoundForDuration(float duration)
    {
        audioSource.Play();
        yield return new WaitForSeconds(duration);
        audioSource.Stop();
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
}
