"""Build CharacterM idle frames and the explicit production terrain control mask."""

from __future__ import annotations

import argparse
from pathlib import Path

import numpy as np
from PIL import Image, ImageOps


IDLE_DIRECTIONS = (
    ("idle-right", "idle-left"),
    ("idle-down-right", "idle-down-left"),
    ("idle-down", None),
    ("idle-up-right", "idle-up-left"),
    ("idle-up", None),
)


def normalize_frame(candidate: Image.Image, canvas_size: int = 256) -> Image.Image:
    alpha = np.asarray(candidate.getchannel("A"))
    active_rows = np.any(alpha > 32, axis=1)
    groups: list[tuple[int, int]] = []
    start: int | None = None
    for row, active in enumerate([*active_rows, False]):
        if active and start is None:
            start = row
        elif not active and start is not None:
            groups.append((start, row))
            start = None
    if groups:
        top, bottom = max(groups, key=lambda group: int((alpha[group[0]:group[1]] > 32).sum()))
        isolated = Image.new("RGBA", candidate.size, (0, 0, 0, 0))
        isolated.alpha_composite(candidate.crop((0, top, candidate.width, bottom)), (0, top))
        candidate = isolated
    bounds = candidate.getchannel("A").getbbox()
    if bounds is None:
        raise RuntimeError("Idle frame is empty")
    frame = candidate.crop(bounds)
    maximum_height = 220
    if frame.width > canvas_size or frame.height > maximum_height:
        scale = min(canvas_size / frame.width, maximum_height / frame.height)
        frame = frame.resize(
            (round(frame.width * scale), round(frame.height * scale)),
            Image.Resampling.LANCZOS,
        )
    canvas = Image.new("RGBA", (canvas_size, canvas_size), (0, 0, 0, 0))
    canvas.alpha_composite(frame, ((canvas_size - frame.width) // 2, 240 - frame.height))
    return canvas


def build_idle_frames(source: Path, output_dir: Path) -> None:
    image = Image.open(source).convert("RGBA")
    output_dir.mkdir(parents=True, exist_ok=True)
    for row, (prefix, mirror_prefix) in enumerate(IDLE_DIRECTIONS):
        top = round(row * image.height / 5)
        bottom = round((row + 1) * image.height / 5)
        for column in range(6):
            left = round(column * image.width / 6)
            right = round((column + 1) * image.width / 6)
            candidate = image.crop((left + 4, top + 4, right - 4, bottom - 4))
            canvas = normalize_frame(candidate)
            canvas.save(output_dir / f"{prefix}-{column}.png")
            if mirror_prefix:
                ImageOps.mirror(canvas).save(output_dir / f"{mirror_prefix}-{column}.png")


def smoothstep(edge0: float, edge1: float, value: np.ndarray) -> np.ndarray:
    unit = np.clip((value - edge0) / (edge1 - edge0), 0.0, 1.0)
    return unit * unit * (3.0 - 2.0 * unit)


def build_control_mask(source: Path, output: Path) -> None:
    macro = Image.open(source).convert("RGB").resize((4096, 2304), Image.Resampling.LANCZOS)
    rgb = np.asarray(macro, dtype=np.float32) / 255.0
    maximum = rgb.max(axis=2)
    minimum = rgb.min(axis=2)
    saturation = maximum - minimum
    luminance = rgb @ np.array([0.2126, 0.7152, 0.0722], dtype=np.float32)
    gray = 1.0 - smoothstep(0.08, 0.24, saturation)
    road = gray * smoothstep(0.10, 0.20, luminance) * (1.0 - smoothstep(0.34, 0.48, luminance))
    concrete = gray * smoothstep(0.38, 0.62, luminance)
    soil = np.clip(1.0 - road - concrete, 0.0, 1.0)
    weights = np.stack((road, soil, concrete), axis=2)
    weights /= np.maximum(weights.sum(axis=2, keepdims=True), 1e-5)
    rgba = np.concatenate((weights, np.ones((*weights.shape[:2], 1), dtype=np.float32)), axis=2)
    output.parent.mkdir(parents=True, exist_ok=True)
    Image.fromarray(np.round(rgba * 255.0).astype(np.uint8), "RGBA").save(output)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--idle-source", type=Path, required=True)
    parser.add_argument("--frames-output", type=Path, required=True)
    parser.add_argument("--terrain-source", type=Path, required=True)
    parser.add_argument("--control-output", type=Path, required=True)
    args = parser.parse_args()
    build_idle_frames(args.idle_source, args.frames_output)
    build_control_mask(args.terrain_source, args.control_output)


if __name__ == "__main__":
    main()
