using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoLoading : MonoBehaviour
{
    public VideoPlayer videoPlayer;



    void Awake()
    {
        StartCoroutine(PrepareVideo());
    }

    IEnumerator PrepareVideo()
    {
        videoPlayer.Prepare();

        // Wait for the video to finish preparing
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        // Video is prepared and can be played
        videoPlayer.Play();
    }
}