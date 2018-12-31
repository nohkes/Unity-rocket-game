using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{   
    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 100f;
    [SerializeField] float levelLoadDelay = 2f;
    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip success;
    [SerializeField] AudioClip death;
    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem successParticles;
    [SerializeField] ParticleSystem deathParticles;



    Rigidbody rigidBody;
    AudioSource audioData;
    enum State { Alive, Dying, Transcending }
    State state = State.Alive;
    bool collisionsDisabled = false;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioData = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(state == State.Alive)
        {
            RotateInput();
            ThrustInput();
        }
        if (Debug.isDebugBuild)
        {
            RespondToDebugKeys();
        }
        
    }

    private void RespondToDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.L)) 
        {
            LoadNextLevel();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
           collisionsDisabled = !collisionsDisabled; 
        }
    }

    void OnCollisionEnter(Collision collision) 
    {
        if (state != State.Alive || collisionsDisabled){ return; }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                //do nothing
                break;
            case "finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartDeathSequence()
    {   
        state = State.Dying;
        audioData.Stop();
        audioData.PlayOneShot(death);
        deathParticles.Play();
        Invoke("RestartLevel", levelLoadDelay);  
        

    }

    private void StartSuccessSequence()
    {
        state = State.Transcending;
        audioData.Stop();
        audioData.PlayOneShot(success);
        successParticles.Play();

        Invoke("LoadNextLevel", levelLoadDelay); //time delay to load next scene
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadNextLevel()
    {   
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings ) 
        {
            nextSceneIndex = 0;
        }
            SceneManager.LoadScene(nextSceneIndex);
        
        
    }

    private void RotateInput()
    {
       rigidBody.freezeRotation = true; // take manual control of rotation
       float rotationThisFrame = rcsThrust * Time.deltaTime;
       

        if (Input.GetKey(KeyCode.A)) //rotating left
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D)) //rotating right
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        };
       rigidBody.freezeRotation = false;

    }

    private void ThrustInput()
    {
        
        if (Input.GetKey(KeyCode.Space)) //can thrust while rotating
        {
            ApplyThrust();
        }
        else
        {
            audioData.Stop();
            mainEngineParticles.Stop();
        }
    }

    private void ApplyThrust()
    {
        float mainBoost = mainThrust * Time.deltaTime;
        rigidBody.AddRelativeForce(Vector3.up * mainBoost);
        if (!audioData.isPlaying)
        {
            audioData.PlayOneShot(mainEngine);
        }
        mainEngineParticles.Play();
    }
}
