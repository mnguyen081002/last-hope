"""Split chroma-keyed animation strips by alpha occupancy and normalize anchors."""

from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image, ImageOps


def active_column_groups(image: Image.Image, threshold: int = 32) -> list[tuple[int, int]]:
    alpha = image.getchannel("A")
    bounds = alpha.getbbox()
    if bounds is None:
        return []
    pixels = alpha.load()
    active = [any(pixels[x, y] > threshold for y in range(image.height)) for x in range(image.width)]
    groups: list[tuple[int, int]] = []
    start: int | None = None
    for x, occupied in enumerate([*active, False]):
        if occupied and start is None:
            start = x
        elif not occupied and start is not None:
            if x - start >= 24:
                groups.append((start, x))
            start = None
    return groups


def split_strip(
    source: Path,
    output_dir: Path,
    prefix: str,
    canvas_size: tuple[int, int],
    baseline: int,
    mirror_prefix: str | None,
    max_frame_height: int,
) -> None:
    image = Image.open(source).convert("RGBA")
    groups = active_column_groups(image)
    if len(groups) != 4:
        raise RuntimeError(f"{source} contains {len(groups)} alpha groups; expected exactly 4: {groups}")

    output_dir.mkdir(parents=True, exist_ok=True)
    for index, (left, right) in enumerate(groups):
        candidate = image.crop((left, 0, right, image.height))
        alpha_bounds = candidate.getchannel("A").getbbox()
        if alpha_bounds is None:
            raise RuntimeError(f"Frame {index} in {source} is empty")
        frame = candidate.crop(alpha_bounds)
        if frame.width > canvas_size[0] or frame.height > max_frame_height:
            scale = min(canvas_size[0] / frame.width, max_frame_height / frame.height)
            frame = frame.resize((round(frame.width * scale), round(frame.height * scale)), Image.Resampling.LANCZOS)

        canvas = Image.new("RGBA", canvas_size, (0, 0, 0, 0))
        position = ((canvas_size[0] - frame.width) // 2, baseline - frame.height)
        canvas.alpha_composite(frame, position)
        canvas.save(output_dir / f"{prefix}-{index}.png")
        if mirror_prefix:
            ImageOps.mirror(canvas).save(output_dir / f"{mirror_prefix}-{index}.png")


def split_grid(
    source: Path,
    output_dir: Path,
    prefix: str,
    canvas_size: tuple[int, int],
    baseline: int,
    mirror_prefix: str | None,
    columns: int,
    rows: int,
    max_frame_height: int,
    cell_inset: int,
) -> None:
    image = Image.open(source).convert("RGBA")
    output_dir.mkdir(parents=True, exist_ok=True)
    frame_index = 0
    for row in range(rows):
        top = round(row * image.height / rows)
        bottom = round((row + 1) * image.height / rows)
        for column in range(columns):
            left = round(column * image.width / columns)
            right = round((column + 1) * image.width / columns)
            candidate = image.crop((
                left + cell_inset, top + cell_inset,
                right - cell_inset, bottom - cell_inset))
            alpha_bounds = candidate.getchannel("A").getbbox()
            if alpha_bounds is None:
                raise RuntimeError(f"Frame {frame_index} in {source} is empty")
            frame = candidate.crop(alpha_bounds)
            if frame.width > canvas_size[0] or frame.height > max_frame_height:
                scale = min(canvas_size[0] / frame.width, max_frame_height / frame.height)
                frame = frame.resize((round(frame.width * scale), round(frame.height * scale)), Image.Resampling.LANCZOS)

            canvas = Image.new("RGBA", canvas_size, (0, 0, 0, 0))
            position = ((canvas_size[0] - frame.width) // 2, baseline - frame.height)
            canvas.alpha_composite(frame, position)
            canvas.save(output_dir / f"{prefix}-{frame_index}.png")
            if mirror_prefix:
                ImageOps.mirror(canvas).save(output_dir / f"{mirror_prefix}-{frame_index}.png")
            frame_index += 1


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--prefix", required=True)
    parser.add_argument("--mirror-prefix")
    parser.add_argument("--canvas", default="256x256")
    parser.add_argument("--baseline", type=int, default=240)
    parser.add_argument("--max-frame-height", type=int, help="Maximum visible sprite height; defaults to baseline")
    parser.add_argument("--grid", help="Split a regular grid such as 4x2 instead of detecting a 4-frame strip")
    parser.add_argument("--cell-inset", type=int, default=0, help="Discard border pixels inside every grid cell")
    args = parser.parse_args()
    width, height = (int(value) for value in args.canvas.lower().split("x", 1))
    max_frame_height = args.max_frame_height or args.baseline
    if args.grid:
        columns, rows = (int(value) for value in args.grid.lower().split("x", 1))
        split_grid(
            args.source, args.output_dir, args.prefix, (width, height), args.baseline,
            args.mirror_prefix, columns, rows, max_frame_height, args.cell_inset)
    else:
        split_strip(
            args.source, args.output_dir, args.prefix, (width, height), args.baseline,
            args.mirror_prefix, max_frame_height)


if __name__ == "__main__":
    main()
