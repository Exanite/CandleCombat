using Project.Source.Input;

namespace Project.Source.Gameplay.Player
{
    public class MlPlayerController : PlayerController
    {
        public PlayerInputData PlayerInputData;
        
        protected override void GatherInput(PlayerInputData input)
        {
            PlayerInputData.CopyTo(input);
        }
    }
}