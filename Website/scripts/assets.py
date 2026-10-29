"""Encode proportionally resized web derivatives; retain the original artwork unchanged."""
from pathlib import Path
from PIL import Image
root=Path(__file__).resolve().parents[1]
for name,width in {'hero-island':1200,'mika':520,'cloud':900,'birds':900,'insects':900,'far-islands':900,'game-day':660,'game-night':660,'game-map':660,'night-world':1100,'icon':256}.items():
    image=Image.open(root/'assets'/f'{name}.png')
    if image.width>width:
        image.thumbnail((width,round(image.height*width/image.width)),Image.Resampling.LANCZOS)
    image.save(root/'dist/assets'/f'{name}.webp','WEBP',quality=86,method=6)

# Open Graph image; keep this portable for Linux build hosts such as Netlify.
with Image.open(root/'assets/social-source.png') as source:
    social=source.convert('RGB')
    social.thumbnail((1200,1200),Image.Resampling.LANCZOS)
    social.save(root/'dist/assets/social.jpg','JPEG',quality=90,optimize=True)
