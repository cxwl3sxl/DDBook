namespace DDBook.EdgeTTS
{
    public enum SessionState
    {
        NotStarted,
        TurnStarted, // turn.start received
        Streaming, // audio binary started
    }
}
