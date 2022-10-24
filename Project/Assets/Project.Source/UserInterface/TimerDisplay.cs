using System;
using TMPro;

namespace Project.Source.UserInterface
{
    public class TimerDisplay : GameUiBehaviour
    {
        public TMP_Text text;

        private void Update()
        {
            text.text = TimeSpan.FromSeconds(GameContext.TimeAlive).ToString(@"mm\:ss\.fff");
        }
    }
}