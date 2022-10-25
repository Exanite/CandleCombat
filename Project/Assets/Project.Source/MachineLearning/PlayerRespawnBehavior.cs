using System;

namespace Project.Source.MachineLearning
{
    [Serializable]
    public enum PlayerRespawnBehavior
    {
        /// <summary>
        /// Respawn players as soon as they die.
        /// </summary>
        Immediate = 0,
        /// <summary>
        /// Respawn players in waves, as determined by TargetInstanceCount.
        /// </summary>
        Waves,
        /// <summary>
        /// Do not respawn players.
        /// </summary>
        None,
    }
}