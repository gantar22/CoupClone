using System.Collections.Generic;

namespace Model.State.Results
{        
    public struct ResultOutcome
    {
        public List<GameStateEdit> edits;
        public string resultText;
        public Phase? newPhase; // empty represents new turn
    }
    public abstract class Result
    {
        protected static readonly List<GameStateEdit> s_NoEdits = new List<GameStateEdit>();
        public abstract ResultOutcome GetResult(GameState gameState, GameConfig config);
    }
}