using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TextRenderer = AELDHKGBIFD;

namespace CheepsAmongUsApi.API
{
    public class CustomText
    {
        public TextRenderer TextRenderer { get; }

        public GameObject gameObject { get; }

        public string Text
        {
            get
            {
                return TextRenderer.Text;
            }

            set
            {
                TextRenderer.Text = value;
            }
        }

        public Color Color
        {
            get
            {
                return TextRenderer.Color;
            }

            set
            {
                TextRenderer.Color = value;
            }
        }

        public Color OutlineColor
        {
            get
            {
                return TextRenderer.OutlineColor;
            }

            set
            {
                TextRenderer.OutlineColor = value;
            }
        }

        public CustomText(float x, float y, string text)
        {
            GameObject go = new GameObject();
            gameObject = go;

            go.transform.SetParent(PlayerHudManager.HudManager.gameObject.transform);
            go.transform.localPosition = new Vector3(x, y);

            TextRenderer = UnityEngine.Object.Instantiate(PlayerHudManager.HudManager.TaskText, go.transform);
            TextRenderer.Text = text;
        }

        public void Destroy()
        {
            SetActive(false);
            UnityEngine.Object.Destroy(gameObject);
        }

        public bool Active
        {
            get
            {
                return gameObject.activeSelf;
            }

            set
            {
                gameObject.SetActive(value);
            }
        }

        public void SetActive(bool active)
        {
            Active = active;
        }
    }
}
