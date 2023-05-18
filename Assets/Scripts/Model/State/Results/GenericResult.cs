namespace Model.State.Results
{
    public class GenericResult : Result // used for cases where we want to edit the result in advance
    {
        public ResultOutcome result;
        
        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            return result;
        }
    }
}