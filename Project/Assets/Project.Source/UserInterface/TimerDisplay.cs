using System;
using TMPro;

namespace Project.Source.UserInterface
{
    public class TimerDisplay : GameUiBehaviour
    {
        public TMP_Text text;

        private void Update()
        {
            if (GameContext == null)
            {
                return;
            }
            
            text.text = TimeSpan.FromSeconds(GameContext.TimeAlive).ToString(@"mm\:ss\.fff");
        }
    }
}