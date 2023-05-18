using System.Collections.Generic;

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

        
        public string title;
        public string description;
        public Justification justification;
        public Results.Result onChosen;

        public Choice(string title, string description, Justification justification, Results.Result onChosen)
        {
            this.title = title;
            this.description = description;
            this.justification = justification;
            this.onChosen = onChosen;
        }
    }


    public struct Phase
    {
        public readonly string text;
        public readonly int choosingPlayer;
        public readonly IEnumerable<Choice> choices;

        public Phase(string text, int choosingPlayer, IEnumerable<Choice> choices)
        {
            this.text = text;
            this.choosingPlayer = choosingPlayer;
            this.choices = choices;
        }
    }
}