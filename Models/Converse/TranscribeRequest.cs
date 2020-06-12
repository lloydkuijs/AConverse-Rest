namespace AConverse_Rest.Models.Converse
{
    public class TranscribeRequest
    {
        public string Audio { get; set; }

        // Currently only supports audio/l16 as of now
        public string AudioType { get; set; } 
        public int SamplingRate { get; set; }
        public int Channels { get; set; }
    }
}
