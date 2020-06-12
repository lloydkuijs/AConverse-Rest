using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using io = System.IO;
using System.IO;
using System.Text;
using YamlDotNet.RepresentationModel;
using AConverse_Rest.Models.Converse;
using System.Media;
using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Watson.SpeechToText.v1;
using IBM.Watson.TextToSpeech.v1;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace AConverse_Rest.Controllers
{

    [ApiController]
    [Route("converse")]
    public class ConverseController : ControllerBase
    {
        private string _cache_path = Path.Combine(AppContext.BaseDirectory, "audio_cache");
        private Dictionary<string, string> _cache = new Dictionary<string, string>();
        private SpeechToTextService _speechToText;
        private TextToSpeechService _textToSpeech;

        public ConverseController(IConfiguration configuration)
        {

            // TODO: temporary, read static cache instead of dynamic switching between cache and real requests
            using (var reader = new StreamReader(Path.Combine(_cache_path, "cache_map.yml")))
            {
                var yaml = new YamlStream();
                yaml.Load(reader);

                // Examine the stream
                var mapping =
                    (YamlMappingNode)yaml.Documents[0].RootNode;

                // TODO: this will NOT load on startup but when a function is called, causing the first function to be somewhat slow with a lot of values in the map
                foreach (var entry in mapping.Children)
                {
                    Console.WriteLine($"Key: {entry.Key}, Value: {entry.Value}");
                    _cache.Add(entry.Key.ToString(), entry.Value.ToString());
                }
            }

            // Text to speech
            var watson_tts_key = configuration["AConverse:TTS:Key"];
            var watson_tts_url = configuration["AConverse:TTS:Url"];

            _textToSpeech = new TextToSpeechService(new IamAuthenticator(apikey: watson_tts_key));
            _textToSpeech.SetServiceUrl(watson_tts_url);


            // Speech to text
            var watson_stt_key = configuration["AConverse:STT:Key"];
            var watson_stt_url = configuration["AConverse:STT:Url"];

            _speechToText = new SpeechToTextService(new IamAuthenticator(apikey: watson_stt_key));

            _speechToText.SetServiceUrl(watson_stt_url);

        }

        [HttpGet]
        public IActionResult PrintCache()
        {
            var audio = io.File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "audio_cache", "Bye.ogg"));

            var result = _speechToText.Recognize(audio, contentType: "audio/ogg");

            return Ok(String.Join("\n", _cache.Select(item => item.Key + ": " + item.Value).ToArray()));
        }

        [HttpPost]
        [Route("synthesize")]
        public async Task<IActionResult> Synthesize([FromBody] SynthesizeRequest request)
        {
            if (_cache.TryGetValue(request.Text, out string filename))
            {
                byte[] audio_data = null;

                // Issues with large files were reported, if this function throws an error check up on ReadAllBytesAsync
                audio_data = await io.File.ReadAllBytesAsync(Path.Combine(_cache_path, filename));

                return Ok(new SynthesizeResponse { Text = request.Text, Audio = audio_data });
            }

            return NotFound("Item was not found in cache, non cache values are currently not supported");
        }

        [HttpPost]
        [Route("transcribe")]
        public IActionResult Transcribe([FromBody] TranscribeRequest request)
        {
            if (request.AudioType != "audio/l16")
            {
                return BadRequest($"Unsupported AudioType given: {request.AudioType}");
            }

            var audio_data = Convert.FromBase64String(request.Audio);

            Console.WriteLine(audio_data);
            
            // TODO: Probably check for 
            string channels = String.Empty;
            string sampling_rate = String.Empty;

            if(request.Channels != 0)
                channels = request.Channels.ToString();
            
            if(request.SamplingRate != 0)
                sampling_rate = request.SamplingRate.ToString();

            var result = _speechToText.Recognize(audio_data, contentType: $"{request.AudioType}; rate={sampling_rate}; channels={channels}");
            
            Debug.WriteLine(result.Response);
            
            return Ok("Item was not found in cache, non cache values are currently not supported");
        }
    }
}