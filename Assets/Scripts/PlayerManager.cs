using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class PlayerController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Movement Settings")]
    public float forwardSpeed = 20f;
    public float acceleration = 0.5f;
    public float maxSpeed = 50f;
    
    public float laneDistance = 1f;
    public float slideSpeed = 10f;

    [Header("Jump Settings")]
    public float jumpForce = 7f;  
    private bool isGrounded = true; 
    private Rigidbody rb;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioClip swipeSound;
    public AudioClip crashSound;
    public AudioClip winSound;
    public AudioClip powerupSound;

    [Header("Visual Effects")]
    public ParticleSystem crashEffect;
    public ParticleSystem speedEffect;
    public ParticleSystem winEffect;

    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private bool stopTouch = false;
    public float swipeRange = 50f; 

    private int currentLane = 1;
    private float score;
    private int highScore;

    // Trigger Handling Fields
    private bool isSpeedModified = false;
    private float speedModifierTimer = 0f;
    private float originSpeed;

    void Start()
    {
        Time.timeScale = 1; 
        rb = GetComponent<Rigidbody>(); 
        originSpeed = forwardSpeed;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateUI();
    }

    void Update()
    {
        // Speed handling
        if (isSpeedModified)
        {
            speedModifierTimer -= Time.deltaTime;
            if (speedModifierTimer <= 0)
            {
                forwardSpeed = originSpeed; 
                isSpeedModified = false;
                if(speedEffect != null && speedEffect.isPlaying) speedEffect.Stop();
            }
        }
        else if (forwardSpeed < maxSpeed)
        {
            forwardSpeed += acceleration * Time.deltaTime;
            // Keep originSpeed updated if we are just accelerating naturally
            originSpeed = forwardSpeed; 
        }

        score = transform.position.x;
        if (score > highScore) highScore = (int)score;
        UpdateUI();

        transform.Translate(Vector3.right * forwardSpeed * Time.deltaTime);

        CheckInput();

        float targetZ = (currentLane - 1) * laneDistance;
        Vector3 currentPos = transform.localPosition;
        
        float newZ = Mathf.Lerp(currentPos.z, targetZ, slideSpeed * Time.deltaTime);
        transform.localPosition = new Vector3(currentPos.x, currentPos.y, newZ);
    }

    void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) MoveRight();
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) MoveLeft();
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) Jump();
        
        if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && !isGrounded) QuickDive();

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            startTouchPosition = Input.GetTouch(0).position;
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Vector2 currentTouchPosition = Input.GetTouch(0).position;
            Vector2 distance = currentTouchPosition - startTouchPosition;

            if (!stopTouch)
            {
                if (Mathf.Abs(distance.x) > Mathf.Abs(distance.y))
                {
                    if (Mathf.Abs(distance.x) > swipeRange) 
                    {
                        if (distance.x < 0) MoveLeft();
                        else MoveRight();
                        stopTouch = true;
                    }
                }
                else
                {
                    if (distance.y > swipeRange) 
                    {
                        Jump();
                        stopTouch = true;
                    }
                    else if (distance.y < -swipeRange)
                    {
                        if (!isGrounded) QuickDive();
                        stopTouch = true;
                    }
                }
            }
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            stopTouch = false;
        }
    }

    void Jump()
    {
        if (isGrounded) 
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            PlaySound(jumpSound);
        }
    }

    void QuickDive()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); // Cancel upward momentum
        rb.AddForce(Vector3.down * jumpForce * 2f, ForceMode.Impulse);
    }

    void MoveLeft()
    {
        if (currentLane < 2) 
        {
            currentLane++;
            PlaySound(swipeSound);
        }
    }

    void MoveRight()
    {
        if (currentLane > 0) 
        {
            currentLane--;
            PlaySound(swipeSound);
        }
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + (int)score;
        if (highScoreText != null) highScoreText.text = "High Score: " + highScore;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Die();
        }

        if (!collision.gameObject.CompareTag("Obstacle"))
        {
            isGrounded = true;
        }
    }

    // Public method for triggers to call
    public void Crash()
    {
        Die();
    }

    public void WinLevel()
    {
        Debug.Log("You Win!");
        PlaySound(winSound);
        if (winEffect != null) winEffect.Play();
        
        // Save score before freezing
        if ((int)score > PlayerPrefs.GetInt("HighScore", 0))
        {
            PlayerPrefs.SetInt("HighScore", (int)score);
            PlayerPrefs.Save();
        }
        WriteScoreToFile();
        
        // Wait a bit or show win screen? For now just freeze like Die but maybe with a different UI?
        // Using existing Panel for simplicity but usually you'd have a WinPanel.
        Time.timeScale = 0; 
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void ChangeSpeed(float multiplier, float duration)
    {
        if (!isSpeedModified)
        {
             originSpeed = forwardSpeed;
        }
        
        forwardSpeed = originSpeed * multiplier;
        speedModifierTimer = duration;
        isSpeedModified = true;

        if (multiplier > 1f)
        {
            Debug.Log("Speed Boost Activated!");
            if (speedEffect != null) 
            {
                speedEffect.Play();
                Debug.Log("Playing Speed Effect: " + speedEffect.gameObject.name);
            }
            else Debug.LogWarning("Speed Effect is missing!");
            
            PlaySound(powerupSound);
        }
        else
        {
             if (speedEffect != null) speedEffect.Stop();
        }
    }

    void Die()
    {
        // Prevent double death
        if (Time.timeScale == 0) return;

        Debug.Log("Player Died!");
        PlaySound(crashSound);
        if (crashEffect != null) 
        {
            crashEffect.Play();
            Debug.Log("Playing Crash Effect");
        }
        else Debug.LogWarning("Crash Effect is missing!");

        if ((int)score > PlayerPrefs.GetInt("HighScore", 0))
        {
            PlayerPrefs.SetInt("HighScore", (int)score);
            PlayerPrefs.Save();
        }
        WriteScoreToFile();
        
        Time.timeScale = 0;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }
    
    // Coroutine removed as requested

    void WriteScoreToFile()
    {
        string path = Application.dataPath + "/scores.txt"; 
        string content = "Date: " + System.DateTime.Now + " | Score: " + (int)score + "\n";
        try { File.AppendAllText(path, content); } catch {}
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}