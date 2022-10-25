using Exanite.Core.Utilities;
using Project.Source.UserInterface;
using TMPro;
using UniDi;

namespace Project.Source.MachineLearning
{
    public class SpectateModeDisplay : GameUiBehaviour
    {
        public TMP_Text Text;

        [Inject]
        private MlUiContext mlUiContext;

        private void Update()
        {
            switch (mlUiContext.SpectateMode)
            {
                case SpectateMode.SpectateSelected:
                {
                    var id = GameContext ? GameContext.Id : "None";
                    Text.text = $"Spectating game: {id}";

                    break;
                }
                case SpectateMode.ViewEntireMap:
                {
                    Text.text = "Viewing entire map";

                    break;
                }
                default: throw ExceptionUtility.NotSupportedEnumValue(mlUiContext.SpectateMode);
            }
        }
    }
}