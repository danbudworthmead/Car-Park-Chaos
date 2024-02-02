using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes.Startup
{
    public class Startup : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadScene("Online");
        }
    }
}
