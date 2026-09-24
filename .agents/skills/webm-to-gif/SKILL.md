---
name: webm-to-gif
description: Convert local WebM animations to looping GIFs with FFmpeg palette generation, while checking actual alpha support and reporting the resulting frame count.
---

# WebM to GIF

Use this skill when a user asks to convert a local WebM animation into a GIF or asks how many images/frames make up the resulting GIF.

## Workflow

1. Resolve the input as a local file and choose a `.gif` output beside it unless the user specifies another path.
2. Run `scripts/convert_webm_to_gif.py`. It uses the bundled `imageio-ffmpeg` executable when available, then falls back to `ffmpeg` on `PATH`.
3. Keep the source dimensions and timing by default. Use `--fps` or `--scale WIDTHxHEIGHT` only when the user requests a different size or cadence. Use `--frames N` to sample exactly N frames while preserving total duration; add `--keep-frame-index I` when a specific source frame must survive sampling.
4. Report the output path, dimensions, duration, and exact GIF frame count. The frame count is read from the generated GIF, not inferred only from the nominal FPS.
5. Explain transparency accurately. GIF supports only binary transparency; inspect the decoded WebM rather than trusting metadata such as `alpha_mode`. If the decoded source has no alpha, preserve the source appearance and do not claim that the output has a transparent background.

## Command

```powershell
python .agents/skills/webm-to-gif/scripts/convert_webm_to_gif.py `
  "path/to/input.webm" `
  --output "path/to/output.gif" `
  --frames 30 `
  --keep-frame-index 68 `
  --force
```

The script requires Python packages `Pillow` and `imageio-ffmpeg`. It uses `palettegen`/`paletteuse` with a reserved transparency entry so real source transparency is retained as far as GIF permits. Do not key out a black background automatically: that changes opaque source pixels and requires an explicit user request.
