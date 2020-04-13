namespace AConverse_Rest.Models.TextToSpeech
{
    public class TTSRequest
    {
        /// <summary>
        /// Text to be converted to speech, also used as a identifier for caching
        /// </summary>
        /// <value></value>
        public string Text { get; set; }

        /// <summary>
        /// If the Spoken sentence should be cached or not
        /// </summary>
        /// <value></value>
        public bool Cache { get; set; }
    }
}