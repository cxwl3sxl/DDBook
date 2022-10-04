namespace DDBook.EdgeTTS
{
    public class Voice
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Gender { get; set; }
        public string Locale { get; set; }
        public string SuggestedCodec { get; set; }
        public string FriendlyName { get; set; }
        public string Status { get; set; }

        public override string ToString()
        {
            return $"{Gender} {ShortName}";
        }
    }
}
