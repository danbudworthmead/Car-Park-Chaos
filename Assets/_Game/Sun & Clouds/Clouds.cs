using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace _Game.Clouds
{
    public class Clouds : MonoBehaviour
    {
        [SerializeField] private UniversalAdditionalLightData sun;
        [SerializeField] private float cloudSpeed;

        private void FixedUpdate()
        {
            sun.lightCookieOffset += Vector2.one * (Time.deltaTime * cloudSpeed);
        }
    }
}
