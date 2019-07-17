using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.UI
{
    public class LoadingText : MonoBehaviour
    {
        public TextMesh Text;
        public float FadeTime;

        private bool _loading;

        public void Activate()
        {
            _loading = true;
            StartCoroutine(Fade());
        }

        public void Deactivate()
        {
            _loading = false;
        }

        public IEnumerator Fade()
        {
            while (_loading)
            {
                yield return FadeTextIn(1.00f);
                yield return FadeTextOut(0.25f);
            }

            yield return FadeTextOut(0.0f);
        }

        public IEnumerator FadeTextIn(float alpha)
        {
            Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, Text.color.a);

            while (Text.color.a < alpha)
            {
                Color c = Text.color;
                float currentTime = (Time.deltaTime / FadeTime);
                Text.color = new Color(c.r, c.g, c.b, c.a + currentTime);
                yield return null;
            }
        }

        public IEnumerator FadeTextOut(float alpha)
        {
            Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, Text.color.a);

            while (Text.color.a > alpha)
            {
                Color c = Text.color;
                float currentTime = (Time.deltaTime / FadeTime);
                Text.color = new Color(c.r, c.g, c.b, c.a - currentTime);
                yield return null;
            }
        }
    }
}
