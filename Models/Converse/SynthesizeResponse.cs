namespace AConverse_Rest.Models.Converse
{
    public class SynthesizeResponse
    {
        /// <summary>
        /// Text to be converted to speech, also used as a identifier for caching
        /// </summary>
        /// <value></value>
        public string Text { get; set; }


        /// <summary>
        /// Audio in .ogg format
        /// </summary>
        /// <value>.ogg formatted byte array</value>
        public byte[] Audio { get; set; }

        public string AudioType { get; set; } = "ogg";
    }
}