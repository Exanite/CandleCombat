using Project.Source.Input;

namespace Project.Source.Gameplay.Player
{
    public class ExternalPlayerController : PlayerController
    {
        public PlayerInputData PlayerInputData { get; } = new PlayerInputData();

        protected override void GatherInput(PlayerInputData input)
        {
            PlayerInputData.CopyTo(input);
        }
    }
}