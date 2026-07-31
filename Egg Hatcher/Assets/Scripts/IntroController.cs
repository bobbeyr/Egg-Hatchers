using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string mainSceneName = "Main"; // Name of your main game scene

    void Start()
    {
        // Subscribe to the video end event
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // Load your main scene after the video finishes
        SceneManager.LoadScene(mainSceneName);
    }
}