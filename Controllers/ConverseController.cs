using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AConverse_Rest.Models.Speech.TextToSpeech;
using Microsoft.AspNetCore.Http;
using io = System.IO;
using System.IO;
using System.Text;
using YamlDotNet.RepresentationModel;
using AConverse_Rest.Models.Converse;

namespace AConverse_Rest.Controllers
{

    [ApiController]
    [Route("converse")]
    public class ConverseController : ControllerBase
    {
        private static string _cache_path = Path.Combine(AppContext.BaseDirectory, "audio_cache");
        private static Dictionary<string, string> _cache = new Dictionary<string, string>();
        static ConverseController()
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
        }

        [HttpGet]
        public IActionResult PrintCache()
        {
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

                return Ok(new TTSResponse { Text = request.Text, Audio = audio_data });
            }

            return NotFound("Item was not found in cache, non cache values are currently not supported");
        }

        [HttpPost]
        [Route("transcribe")]
        public IActionResult Transcribe([FromBody] TranscribeRequest request)
        {   
            return Ok("Item was not found in cache, non cache values are currently not supported");
        }
    }
}