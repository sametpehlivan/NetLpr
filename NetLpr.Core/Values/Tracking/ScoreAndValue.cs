namespace NetLpr.Core.Values.Tracking
{
    public record ScoreAndValue
    {
        public string Value { get; private set; } = string.Empty;
        public float Score { get; private set; } = .0f;
        public ScoreAndValue(string value, float score)
        {
            Value = value;
            Score = score;
        }

    }
}
