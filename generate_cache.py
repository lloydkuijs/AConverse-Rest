import sys
from pathlib import Path
import pathlib
import unicodedata
import re

import markdown
import ruamel.yaml as yaml
from html.parser import HTMLParser
from ibm_watson import text_to_speech_v1
from ibm_cloud_sdk_core.authenticators import iam_authenticator


class ResponseHTMLParser(HTMLParser):

    def __init__(self):
        HTMLParser.__init__(self)
        self.responses = []
        self._read = False

    def handle_starttag(self, tag, attrs):
        if(tag == 'li'):
            self._read = True

    def handle_endtag(self, tag):
        pass

    # Checks if a response is dynamic or static
    def _is_dynamic(self, response: str):
        return True in [c in response for c in "}{)("]

    def handle_data(self, data):
        if(self._read):
            response = str.strip(data)
            # String not empty and is not already in
            if(response and response not in self.responses):
                if(self._is_dynamic(response)):
                    return None

                self.responses.append(response)
                self._read = False


def read_arguments():
    # TODO: allow multiple argument input and generate cache. Also allow folder path?
    try:
        # Get the filename we want to generate/ add to the cache
        settings_argument = sys.argv[1]
    except IndexError:
        settings_argument = "response_map.md"  # Default sentences file

    return settings_argument


def read_responses(file_path: str):
    content = Path(file_path).read_text()

    html = markdown.markdown(content)

    parser = ResponseHTMLParser()

    parser.feed(html)

    return parser.responses


def sanitize_response(response: str):
    return get_valid_filename(response)


def initialize_watsonTTS():
    # Open the key storage
    with open("_config.yml", 'r') as f:

        # Might cause performance issues later if there are a lot of keys (although unlikely)
        data = yaml.safe_load(f)

        ibm_watson = data['IBM_WATSON']

        # When using different types of services from IBM WATSON, exchange this with IBM_WATSON.TTS.(-) in the yaml file
        key = ibm_watson['key']
        url = ibm_watson['url']

        authenticator = iam_authenticator.IAMAuthenticator(key)

        text_to_speech = text_to_speech_v1.TextToSpeechV1(
            authenticator=authenticator
        )

        text_to_speech.set_service_url(url)

        return text_to_speech


def get_valid_filename(s):
    s = str(s).strip().replace(' ', '_')
    return re.sub(r'(?u)[^-\w.]', '', s)


def generate_audio_file(file_path: Path, sentence: str, text_to_speech: text_to_speech_v1.TextToSpeechV1):
    with file_path.open('wb') as audio_file:
        audio_file.write(
            text_to_speech.synthesize(
                sentence,
                voice='en-US_AllisonV3Voice',
                accept='audio/ogg'
            ).get_result().content)


def update_cache(cache_path: Path, responses: list):
    # audio_cache/sentence_map.yml
    cache_map_path = cache_path / "cache_map.yml"

    cache = yaml.safe_load(cache_map_path.open('r+'))

    if(cache is not None):
        for c_key in list(cache.keys()):

            if(c_key not in responses):
                # TODO: selectable cache extension
                c_file = cache_path / (c_key + '.ogg')

                del cache[c_key]

                if(c_file.is_file()):
                    c_file.unlink()

    if(cache is None):
        cache = {}

    tts = initialize_watsonTTS()

    for response in responses:
        sanitized_response = sanitize_response(response)
        filename = sanitized_response + '.ogg'  # TODO: selectable cache extension
        file_path = cache_path / filename

        if(response not in cache):
            cache[response] = filename

        else:
            if(not file_path.is_file()):
                print("Generating audio file for {}".format(filename))
                generate_audio_file(file_path, response, tts)
            else:
                print("File already exists, {}".format(filename))

    with cache_map_path.open('w') as cm:
        yaml.dump(cache, cm, default_flow_style=False)


if __name__ == "__main__":

    audio_extension = '.ogg'

    file_path = read_arguments()

    responses = read_responses(file_path)

    cache_path = Path("audio_cache")
    # Check if audio_cache folder already exists
    try:
        cache_path.mkdir()
    except FileExistsError:
        print("Cache folder already exists, adding new files to it")

    update_cache(cache_path, responses)
