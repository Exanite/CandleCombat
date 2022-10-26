using System.Collections.Generic;

namespace Project.Source.MachineLearning.Data
{
    public class MlOutput
    {
        public readonly List<MlGameOutput> GameOutputs = new List<MlGameOutput>();

        public readonly List<MlGameStartedEvent> StartedGames = new List<MlGameStartedEvent>();
        public readonly List<MlGameClosedEvent> ClosedGames = new List<MlGameClosedEvent>();
    }
}