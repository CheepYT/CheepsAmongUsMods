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

        internal static void StartNotifier()
        {
            LobbyBehaviourStartedEvent.Listener += () =>
            {
                Text = null;
            };
        }

        private static Task NotificationTask = null;

        public static void CancelNotification()
        {
            try
            {
                if (NotificationTask != null)
                {
                    NotificationTask.Dispose();
                    Text.SetActive(false);
                    NotificationTask = null;
                }
            }
            catch { }
        }

        public static void ShowNotification(string text, long time)
        {
            if (Text == null) {
                Text = new CustomText(TextX, TextY, text);
                Text.TextRenderer.Centered = true;
                Text.TextRenderer.scale = TextScale;
            }

            CancelNotification();

            Text.Text = text;

            NotificationTask = Task.Run(async () =>
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
                }
            });

        }
    }
}
