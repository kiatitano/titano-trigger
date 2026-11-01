from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'assets' / 'icon.ico'
OUT.parent.mkdir(exist_ok=True)

size = 512
image = Image.new('RGBA', (size, size), (0, 0, 0, 0))
draw = ImageDraw.Draw(image)
draw.rounded_rectangle((25, 25, 487, 487), radius=118, fill=(26, 23, 59, 255))
glow = Image.new('RGBA', (size, size), (0, 0, 0, 0))
g = ImageDraw.Draw(glow)
g.ellipse((95, 52, 430, 410), fill=(143, 94, 255, 165))
glow = glow.filter(ImageFilter.GaussianBlur(62))
image = Image.alpha_composite(image, glow)
draw = ImageDraw.Draw(image)
draw.rounded_rectangle((25, 25, 487, 487), radius=118, outline=(180, 147, 255, 180), width=5)
bolt = [(287, 74), (115, 283), (238, 283), (219, 441), (402, 199), (278, 199)]
draw.polygon(bolt, fill=(208, 185, 255, 255))
draw.line(bolt + [bolt[0]], fill=(244, 235, 255, 255), width=8, joint='curve')
image.save(OUT, format='ICO', sizes=[(16,16),(24,24),(32,32),(48,48),(64,64),(128,128),(256,256)])
print(OUT)
