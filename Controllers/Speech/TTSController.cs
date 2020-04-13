using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AConverse_Rest.Models.TextToSpeech;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;

namespace AConverse_Rest.Controllers.Speech
{

    [ApiController]
    [Route("tts")]
    public class TTSController : ControllerBase
    {
        private static string _cachePath = Path.Combine(AppContext.BaseDirectory, "cache");
        static TTSController()
        {
            // If this causes memory issues later, try Directory.EnumerateFiles
            foreach (var fullPath in Directory.GetFiles(_cachePath))
            {
                var fileName = Path.GetFileNameWithoutExtension(fullPath);

                var encodedName = EncodeFileName(fileName);
            }
        }

        [HttpPost]
        [Route("synthesize")]
        public IActionResult Synthesize([FromBody] TTSRequest request)
        {
            var encoded = EncodeFileName(request.Text);

            return Ok("Hello world.");
        }

        private static string EncodeFileName(string fileName)
        {
            Encoding unicode = Encoding.Unicode;

            // File system is Unicode
            var bytes = unicode.GetBytes(fileName);

            // Use base64 to encode the bytes https://en.wikipedia.org/wiki/Base64
            return Convert.ToBase64String(bytes);
        }

        private static string DecodeFileName(string fileName)
        {
            Encoding unicode = Encoding.Unicode;

            // Use base64 to decode the string to bytes https://en.wikipedia.org/wiki/Base64
            var bytes = Convert.FromBase64String(fileName);

            // File system is Unicode
            return unicode.GetString(bytes);
        }
    }
}