using TMPro;

namespace Project.Source.UserInterface
{
    public class SpectatedGameDisplay : GameUiBehaviour
    {
        public TMP_Text Text;

        private void Update()
        {
            var id = GameContext ? GameContext.Id : "None";
            
            Text.text = $"Spectated game: {id}";
        }
    }
}