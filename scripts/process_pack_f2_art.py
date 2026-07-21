"""Prepare Pack F2 terrain and aligned exterior/interior building sprites."""

from pathlib import Path
from PIL import Image

ROOT = Path("Assets/Art/Production/WorldF2")
NAMES = [
    "grocery", "clinic", "military-checkpoint",
    "workshop", "apartment", "warehouse",
    "water-station", "rescue-station", "utility-substation",
]


def grid_cell(image: Image.Image, index: int) -> Image.Image:
    column, row = index % 3, index // 3
    return image.crop((
        round(column * image.width / 3), round(row * image.height / 3),
        round((column + 1) * image.width / 3), round((row + 1) * image.height / 3),
    ))


def subject(image: Image.Image) -> Image.Image:
    bounds = image.getchannel("A").getbbox()
    if bounds is None:
        raise RuntimeError("Empty building cell")
    return image.crop(bounds)


def place(image: Image.Image, scale: float) -> Image.Image:
    image = image.resize((round(image.width * scale), round(image.height * scale)), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (512, 512), (0, 0, 0, 0))
    canvas.alpha_composite(image, ((512 - image.width) // 2, 490 - image.height))
    return canvas


def process_terrain() -> None:
    image = Image.open(ROOT / "Sources/world-f2-terrain-source.png").convert("RGB")
    target_ratio = 16 / 9
    current_ratio = image.width / image.height
    if current_ratio > target_ratio:
        width = round(image.height * target_ratio)
        left = (image.width - width) // 2
        image = image.crop((left, 0, left + width, image.height))
    else:
        height = round(image.width / target_ratio)
        top = (image.height - height) // 2
        image = image.crop((0, top, image.width, top + height))
    image.resize((2048, 1152), Image.Resampling.LANCZOS).save(ROOT / "Terrain/world-f2-terrain.png")


def process_buildings() -> None:
    exterior = Image.open(ROOT / "Sources/buildings-exterior-alpha.png").convert("RGBA")
    interior = Image.open(ROOT / "Sources/buildings-interior-alpha.png").convert("RGBA")
    for index, name in enumerate(NAMES):
        outside = subject(grid_cell(exterior, index))
        inside = subject(grid_cell(interior, index))
        scale = min(470 / max(outside.width, inside.width), 460 / max(outside.height, inside.height))
        place(outside, scale).save(ROOT / f"Buildings/Exterior/{name}-exterior.png")
        place(inside, scale).save(ROOT / f"Buildings/Interior/{name}-interior.png")


if __name__ == "__main__":
    process_terrain()
    process_buildings()
