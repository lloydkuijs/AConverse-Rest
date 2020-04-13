import sys
from pathlib import Path

import markdown

# Not working yet, will be added in a newer version
if __name__ == "__main__":
    # TODO: allow multiple argument input and generate cache. Also allow folder path?
    try:
        # Get the filename we want to generate/ add to the cache
        settings_argument = sys.argv[1]
    except IndexError:
        settings_argument = "response_map.md"  # Default sentences file

    file_path = Path.cwd() / settings_argument

    content = Path(file_path).read_text()

    html = markdown.markdown(content)

    print(html) 
    
    # TODO: Generate cache here

