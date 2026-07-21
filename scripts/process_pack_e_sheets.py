"""Split Pack E chroma-key sheets into anchor-stable Unity sprites."""

from __future__ import annotations

from pathlib import Path
from PIL import Image


def cell(image: Image.Image, column: int, row: int, columns: int, rows: int) -> Image.Image:
    left = round(column * image.width / columns)
    right = round((column + 1) * image.width / columns)
    top = round(row * image.height / rows)
    bottom = round((row + 1) * image.height / rows)
    return image.crop((left, top, right, bottom))


def subject(image: Image.Image) -> Image.Image:
    bounds = image.getchannel("A").getbbox()
    if bounds is None:
        raise RuntimeError("Empty sprite cell")
    return image.crop(bounds)


def normalized(image: Image.Image, canvas: tuple[int, int], baseline: int, scale: float) -> Image.Image:
    resized = image.resize((max(1, round(image.width * scale)), max(1, round(image.height * scale))), Image.Resampling.LANCZOS)
    result = Image.new("RGBA", canvas, (0, 0, 0, 0))
    x = (canvas[0] - resized.width) // 2
    y = baseline - resized.height
    result.alpha_composite(resized, (x, y))
    return result


def containers() -> None:
    source = Image.open("Assets/Art/Production/LootPackE/Sources/containers-sheet-alpha.png").convert("RGBA")
    output = Path("Assets/Art/Production/LootPackE/Containers")
    output.mkdir(parents=True, exist_ok=True)
    names = ["supply-bag", "medical-cabinet", "toolbox", "industrial-locker", "military-cache"]
    for column, name in enumerate(names):
        pair = []
        for row in range(2):
            source_cell = cell(source, column, row, 5, 2)
            # The open locker door nearly reaches the next cell; trim the neighboring cache sliver.
            if column == 3 and row == 1:
                source_cell = source_cell.crop((0, 0, source_cell.width - 25, source_cell.height))
            pair.append(subject(source_cell))
        scale = min(340 / max(image.width for image in pair), 340 / max(image.height for image in pair))
        for row, state in enumerate(("closed", "open")):
            normalized(pair[row], (384, 384), 366, scale).save(output / f"{name}-{state}.png")


def items() -> None:
    source = Image.open("Assets/Art/Production/LootPackE/Sources/items-sheet-alpha.png").convert("RGBA")
    output = Path("Assets/Art/Production/LootPackE/Items")
    output.mkdir(parents=True, exist_ok=True)
    names = ["food", "water", "material", "filter", "medicine"]
    for column, name in enumerate(names):
        image = subject(cell(source, column, 0, 5, 1))
        scale = min(220 / image.width, 220 / image.height)
        normalized(image, (256, 256), 238, scale).save(output / f"item-{name}.png")


def props() -> None:
    source = Image.open("Assets/Art/Production/WorldProps/Sources/props-sheet-alpha.png").convert("RGBA")
    output = Path("Assets/Art/Production/WorldProps/Sprites")
    output.mkdir(parents=True, exist_ok=True)
    names = ["radiation-sign", "streetlamp", "barrels", "broken-fence", "dead-scrub", "tarp", "concrete-rubble", "warning-beacon"]
    for index, name in enumerate(names):
        image = subject(cell(source, index % 4, index // 4, 4, 2))
        scale = min(340 / image.width, 340 / image.height)
        normalized(image, (384, 384), 366, scale).save(output / f"prop-{name}.png")


if __name__ == "__main__":
    containers()
    items()
    props()
