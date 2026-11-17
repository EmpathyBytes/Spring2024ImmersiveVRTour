using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmpathyBytes.Buzz
{
    /// <summary>
    /// Adds the "Ask Buzz" summon button to the real tour scenes automatically when they load, so no scene file
    /// needs hand-editing (and teammates editing those scenes cannot conflict with it).
    /// Buzz himself is only built the first time he is summoned.
    /// </summary>
    public static class BuzzTourBootstrap
    {
        private static readonly HashSet<string> TourScenes = new HashSet<string>
        {
            "Main Menu", "Freshman Dorm", "Reading Room", "Music Room", "Paper Museum", "1996 Olympics Room"
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            SceneManager.sceneLoaded -= OnLoaded;
            SceneManager.sceneLoaded += OnLoaded;
            Spawn(SceneManager.GetActiveScene());
        }

        private static void OnLoaded(Scene scene, LoadSceneMode mode) { Spawn(scene); }

        private static void Spawn(Scene scene)
        {
            if (!TourScenes.Contains(scene.name)) return;
            if (Object.FindObjectOfType<BuzzTourController>() != null || Object.FindObjectOfType<BuzzGuide>() != null) return;
            new GameObject("BuzzTourController").AddComponent<BuzzTourController>();
        }
    }
}
