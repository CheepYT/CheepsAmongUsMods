using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.Enumerations;
using CheepsAmongUsMod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnonGameMode
{
    public class AnonGameMode : GameMode
    {
        public AnonGameMode() : base(10, Anon.GameModeName) { }

        public override void OnStart()
        {
            #region --------- Unset all style options ---------
            Task.Run(async () =>
            {
                await Task.Delay(1500); // Wait a bit

                var player = PlayerController.LocalPlayer;

                player.RpcSetColor(ColorType.Black);
                player.RpcSetHat(HatType.NoHat);
                player.RpcSetSkin(SkinType.None);
                player.RpcSetPet(PetType.NoPet);
                player.RpcSetName("");
            });
            #endregion

            base.OnStart();
        }
    }
}
