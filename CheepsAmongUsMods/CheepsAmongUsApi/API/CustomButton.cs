using CheepsAmongUsApi.API.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CheepsAmongUsApi.API
{
    public class CustomButton
    {
        public static List<CustomButton> AllButtons = new List<CustomButton>();

        internal static void StartManager()
        {
            HudUpdateEvent.Listener += () =>
            {
                foreach (var btn in AllButtons)
                    try
                    {
                        if (btn.InBounds && Functions.GetKeyDown(KeyCode.Mouse0))
                            btn.CallClick();
                    }
                    catch { }
            };
        }

        public event Notify OnClick;

        public GameObject gameObject { get; }

        public SpriteRenderer SpriteRenderer { get; }

        public CustomButton(float x, float y, Sprite sprite)
        {
            GameObject go = new GameObject();
            gameObject = go;

            go.transform.SetParent(PlayerHudManager.HudManager.gameObject.transform);
            go.transform.localPosition = new Vector3(x, y);

            SpriteRenderer = go.AddComponent<SpriteRenderer>();
            SpriteRenderer.sprite = sprite;

            AllButtons.Add(this);
        }

        private void CallClick()
        {
            OnClick?.Invoke();
        }

        public void Destroy()
        {
            if(AllButtons.Contains(this))
                AllButtons.Remove(this);

            gameObject.SetActive(false);
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

        public bool InBounds
        {
            get
            {
                var mousePos = Camera.main.ScreenToWorldPoint(Functions.MousePosition);
                var itemPos = SpriteRenderer.transform.position;
                var itemScale = SpriteRenderer.size;

                bool inBoundsX = itemPos.x - itemScale.x / 2 < mousePos.x && itemPos.x + itemScale.x / 2 > mousePos.x;
                bool inBoundsY = itemPos.y - itemScale.y / 2 < mousePos.y && itemPos.y + itemScale.y / 2 > mousePos.y;
                return inBoundsX && inBoundsY && gameObject.activeSelf;
            }
        }
    }
}
