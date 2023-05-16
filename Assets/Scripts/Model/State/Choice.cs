using System;
using System.Collections.Generic;
using Util;

namespace Model.State
{
    public struct Choice
    {
        public enum Justification
        {
            Free,
            Forced,
            UseCard,
            Bluff,
        }
        public struct Result
        {
            public List<GameStateEdit> edits;
            public string resultText;
            public Phase? newPhase; // empty represents new turn
        }
        
        public string title;
        public string description;
        public Justification justification;
        public Func<Result> result;
    }

    public struct Phase
    {
        public string text;
        public int choosingPlayer;
        public IEnumerable<Choice> choices;
    }
}