"""
Batch remove white background from PNG images, making them transparent.
Only removes white pixels connected to the image edges (flood-fill from borders).
White pixels inside the graphic are preserved.

Usage: python remove_white_bg.py [options]
"""

import os
import sys
import argparse
from collections import deque
from pathlib import Path
from PIL import Image


def remove_white_background(
    input_path: Path,
    output_path: Path,
    threshold: int = 240,
    edge_smooth: bool = True,
):
    """
    Remove white/near-white background from a PNG image using edge-flood-fill.
    Only white pixels connected to the image borders are removed;
    white pixels enclosed by non-white graphics are preserved.

    Args:
        input_path: Source PNG file
        output_path: Destination PNG file
        threshold: Pixels with max(R,G,B) >= this are considered "white"
        edge_smooth: If True, semi-white edge pixels get proportional alpha
    """
    img = Image.open(input_path).convert("RGBA")
    pixels = img.load()
    w, h = img.size

    # ---------------------------------------------------------------
    # Step 1: Flood-fill from all 4 edges to find "background" pixels.
    # Only white/near-white pixels connected to the border are marked.
    # ---------------------------------------------------------------
    visited = [[False] * w for _ in range(h)]
    queue = deque()

    # Seed: all edge pixels that are white
    for x in range(w):
        r, g, b, a = pixels[x, 0]
        if a > 0 and max(r, g, b) >= threshold:
            visited[0][x] = True
            queue.append((x, 0))
        r, g, b, a = pixels[x, h - 1]
        if a > 0 and max(r, g, b) >= threshold:
            visited[h - 1][x] = True
            queue.append((x, h - 1))

    for y in range(1, h - 1):
        r, g, b, a = pixels[0, y]
        if a > 0 and max(r, g, b) >= threshold:
            visited[y][0] = True
            queue.append((0, y))
        r, g, b, a = pixels[w - 1, y]
        if a > 0 and max(r, g, b) >= threshold:
            visited[y][w - 1] = True
            queue.append((w - 1, y))

    # 8-directional flood fill
    dirs = [(-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1)]

    while queue:
        x, y = queue.popleft()
        for dx, dy in dirs:
            nx, ny = x + dx, y + dy
            if 0 <= nx < w and 0 <= ny < h and not visited[ny][nx]:
                r, g, b, a = pixels[nx, ny]
                if a > 0 and max(r, g, b) >= threshold:
                    visited[ny][nx] = True
                    queue.append((nx, ny))

    # ---------------------------------------------------------------
    # Step 2: Make visited (background) pixels transparent.
    # ---------------------------------------------------------------
    if edge_smooth:
        for y in range(h):
            for x in range(w):
                if not visited[y][x]:
                    continue
                r, g, b, a = pixels[x, y]
                max_val = max(r, g, b)
                whiteness = (max_val - threshold) / (255 - threshold)
                whiteness = max(0.0, min(1.0, whiteness))
                new_alpha = int(a * (1.0 - whiteness))

                if new_alpha > 0:
                    factor = threshold / (255 - threshold + 1)
                    rr = int(r - max_val * whiteness + factor)
                    gg = int(g - max_val * whiteness + factor)
                    bb = int(b - max_val * whiteness + factor)
                    pixels[x, y] = (
                        max(0, min(255, rr)),
                        max(0, min(255, gg)),
                        max(0, min(255, bb)),
                        max(0, min(255, new_alpha)),
                    )
                else:
                    pixels[x, y] = (0, 0, 0, 0)
    else:
        for y in range(h):
            for x in range(w):
                if visited[y][x]:
                    pixels[x, y] = (0, 0, 0, 0)

    img.save(output_path, "PNG")


def batch_process(
    input_dir: Path,
    output_dir: Path,
    threshold: int = 240,
    edge_smooth: bool = True,
    overwrite: bool = False,
):
    """
    Process all PNG files in input_dir recursively.
    """
    png_files = list(input_dir.rglob("*.png"))
    if not png_files:
        print(f"No PNG files found in {input_dir}")
        return

    print(f"Found {len(png_files)} PNG file(s) in {input_dir}")
    print(f"Method: edge-flood-fill, Threshold: RGB max >= {threshold}, Edge Smooth: {edge_smooth}")
    print(f"Output dir: {output_dir}")
    print("-" * 50)

    success = 0
    skipped = 0
    failed = 0

    for png_file in png_files:
        rel_path = png_file.relative_to(input_dir)
        out_file = output_dir / rel_path

        if not overwrite and out_file.exists():
            print(f"  SKIP (exists): {rel_path}")
            skipped += 1
            continue

        try:
            out_file.parent.mkdir(parents=True, exist_ok=True)
            remove_white_background(png_file, out_file, threshold, edge_smooth)
            print(f"  OK: {rel_path}")
            success += 1
        except Exception as e:
            print(f"  FAIL: {rel_path} - {e}")
            failed += 1

    print("-" * 50)
    print(f"Done. Success: {success}, Skipped: {skipped}, Failed: {failed}")


def main():
    parser = argparse.ArgumentParser(
        description="Batch remove white background from PNG images (edge-based flood-fill)"
    )
    parser.add_argument(
        "--input", "-i", default=None,
        help="Input directory (default: ./images)",
    )
    parser.add_argument(
        "--output", "-o", default=None,
        help="Output directory (default: ./images_transparent)",
    )
    parser.add_argument(
        "--threshold", "-t", type=int, default=240,
        help="Whiteness threshold (0-255). Pixels with max(R,G,B) >= this are flood-filled. Default: 240",
    )
    parser.add_argument(
        "--no-smooth", action="store_true",
        help="Disable edge smoothing (binary cut-off, no anti-alias)",
    )
    parser.add_argument(
        "--overwrite", action="store_true",
        help="Overwrite existing output files",
    )
    parser.add_argument(
        "--in-place", action="store_true",
        help="Overwrite original files directly (DANGER: no backup)",
    )

    args = parser.parse_args()

    script_dir = Path(__file__).parent.resolve()
    input_dir = Path(args.input) if args.input else script_dir / "images"
    output_dir = (
        Path(args.output) if args.output
        else (input_dir if args.in_place else script_dir / "images_transparent")
    )

    if not input_dir.exists():
        print(f"Error: Input directory not found: {input_dir}")
        sys.exit(1)

    if args.in_place:
        print("WARNING: Running in --in-place mode. Original files will be overwritten!")
        resp = input("Continue? (y/N): ").strip().lower()
        if resp != "y":
            print("Cancelled.")
            sys.exit(0)

    batch_process(
        input_dir=input_dir,
        output_dir=output_dir,
        threshold=args.threshold,
        edge_smooth=not args.no_smooth,
        overwrite=args.overwrite or args.in_place,
    )


if __name__ == "__main__":
    main()
