using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class testAudioScript : MonoBehaviour
{
    [SerializeField] private AudioSource audio;
    public bool flag;
    [SerializeField] private string sceneName;
    // Start is called before the first frame update
    void Start()
    {
        flag = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (flag)
        {
            StartCoroutine(SoundAndSwitch());
        }
    }
    private IEnumerator SoundAndSwitch()
    {
        if (audio != null)
        {
            audio.Play();
            yield return new WaitForSeconds(audio.clip.length);
            SceneManager.LoadScene(sceneName);
        }
    }
}
