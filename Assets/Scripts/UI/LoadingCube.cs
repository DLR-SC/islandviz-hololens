using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.UI.Component
{
    public class LoadingCube : UIComponent
    {
        public Material Material;
        public float FadeTime;
        public float Stepsize;
        public int Steps;

        private bool _loading;

        public override IEnumerator Activate()
        {
            _loading = true;
            StartCoroutine(Rotate());
            yield break;
        }

        public override IEnumerator Deactivate()
        {
            _loading = false;
            yield break;
        }

        public IEnumerator Rotate()
        {
            yield return FadeIn(1.0f);

            Vector3 startAngle = new Vector3(-45, 0, 45);
            Vector3 targetAngle = new Vector3(-225, 0, 225);
            Vector3 tempAngle = startAngle;

            while (_loading)
            {
                tempAngle = startAngle;

                for (int step = 0 - (Steps / 2); step < Steps / 2; step++)
                {
                    float t = Sigmoid(step * Stepsize);
                    tempAngle = Vector3.Lerp(tempAngle, targetAngle, t);
                    transform.localRotation = Quaternion.Euler(tempAngle);
                    yield return new WaitForSeconds(0.02f);
                }

                tempAngle = startAngle;
            }

            yield return FadeOut(0.0f);
        }

        public IEnumerator FadeIn(float alpha)
        {
            Material.color = new Color(Material.color.r, Material.color.g, Material.color.b, Material.color.a);

            while (Material.color.a < alpha)
            {
                Color c = Material.color;
                float currentTime = (Time.deltaTime / FadeTime);
                Material.color = new Color(c.r, c.g, c.b, c.a + currentTime);
                yield return null;
            }
        }

        public IEnumerator FadeOut(float alpha)
        {
            Material.color = new Color(Material.color.r, Material.color.g, Material.color.b, Material.color.a);

            while (Material.color.a > alpha)
            {
                Color c = Material.color;
                float currentTime = (Time.deltaTime / FadeTime);
                Material.color = new Color(c.r, c.g, c.b, c.a - currentTime);
                yield return null;
            }
        }

        private float Sigmoid(float x)
        {
            return 1.0f / (1.0f + Mathf.Exp(-x));
        }
    }
}
