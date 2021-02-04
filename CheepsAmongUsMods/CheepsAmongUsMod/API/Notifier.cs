using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheepsAmongUsMod.API
{ 
    public class Notifier
    {
        public static CustomText Text = null;

        public const float TextX = 0f;
        public const float TextY = 2f;
        public const float TextScale = 1.5f;

        public static bool IsNotifying = false;

        public static bool CancelNotification = false;

        internal static void StartNotifier()
        {
            LobbyBehaviourStartedEvent.Listener += () =>
            {
                Text = null;
            };
        }

        public static void ShowNotification(string text, long time)
        {
            if (Text == null) {
                Text = new CustomText(TextX, TextY, text);
                Text.TextRenderer.Centered = true;
                Text.TextRenderer.scale = TextScale;
            }

            Text.Text = text;

            if (IsNotifying)
                return;

            IsNotifying = true;

            Task.Run(async () =>
            {
                Text.SetActive(true);
                Text.Color = new UnityEngine.Color(1, 1, 1, 0);

                CustomStopwatch stopwatch = new CustomStopwatch(true);

                while(stopwatch.ElapsedMilliseconds < time)
                {
                    float a = stopwatch.ElapsedMilliseconds / (time / 2f);

                    if (stopwatch.ElapsedMilliseconds > (time / 2))
                        a = (time - stopwatch.ElapsedMilliseconds) / (time / 2f);

                    Text.Color = new UnityEngine.Color(1, 1, 1, a);

                    await Task.Delay(1);

                    if (CancelNotification)
                    {
                        CancelNotification = false;

                        Text.SetActive(false);
                        return;
                    }
                }

                IsNotifying = false;
            });

        }
    }
}
