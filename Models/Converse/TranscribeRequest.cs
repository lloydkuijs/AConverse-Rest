namespace AConverse_Rest.Models.Converse
{
    public class TranscribeRequest
    {
        public byte[] Audio { get; set; }
        public string Format { get; set; }
    }
}
