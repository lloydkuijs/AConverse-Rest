namespace AConverse_Rest.Models.TextToSpeech
{
    public class TTSR
    {
        /// <summary>
        /// Text to be converted to speech, also used as a identifier for caching
        /// </summary>
        /// <value></value>
        public string Text { get; set; }


        /// <summary>
        /// Audio in MP3 format
        /// </summary>
        /// <value>MP3 formatted byte array</value>
        public byte[] Audio { get; set; }
    }
}