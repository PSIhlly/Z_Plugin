---
name: webm-to-gif
description: Convert local WebM animations to looping GIFs with FFmpeg palette generation, while checking actual alpha support and reporting the resulting frame count.
---

# WebM to GIF

Use this skill when a user asks to convert a local WebM animation into a GIF or asks how many images/frames make up the resulting GIF.

## Workflow

1. Resolve the input as a local file and choose a `.gif` output beside it unless the user specifies another path.
2. Run `scripts/convert_webm_to_gif.py`. It uses the bundled `imageio-ffmpeg` executable when available, then falls back to `ffmpeg` on `PATH`; use `--ffmpeg` to select another build. VP8/VP9 WebM with alpha needs an FFmpeg build with the `libvpx`/`libvpx-vp9` decoder. The script selects this decoder before every input read, including inspection and conversion, because FFmpeg's native VP8/VP9 decoders can silently discard alpha.
3. Keep the source dimensions and timing by default. Use `--fps` or `--scale WIDTHxHEIGHT` only when the user requests a different size or cadence. Use `--frames N` to sample exactly N frames while preserving total duration; add `--keep-frame-index I` when a specific source frame must survive sampling.
4. Report the output path, dimensions, duration, and exact GIF frame count. Read the frame count and transparent pixels from the generated GIF itself. A transparency palette entry alone does not prove that any pixel uses it.
5. Explain transparency accurately. GIF supports only binary transparency. Inspect decoded alpha rather than trusting `alpha_mode` alone; if a VP8/VP9 stream declares alpha but the selected FFmpeg lacks the matching libvpx decoder, stop with an error. If the decoded source has transparent pixels but the GIF has none, reject the output. If the decoded source has no alpha, preserve the source appearance and do not claim a transparent background.

## Command

```powershell
python .agents/skills/webm-to-gif/scripts/convert_webm_to_gif.py `
  "path/to/input.webm" `
  --output "path/to/output.gif" `
  --ffmpeg "path/to/ffmpeg.exe"
```

The script requires Python package `Pillow` and an FFmpeg executable; `imageio-ffmpeg` is optional if FFmpeg is on `PATH` or supplied with `--ffmpeg`. It uses `palettegen`/`paletteuse` with a reserved transparency entry and an alpha threshold of 128. It writes a temporary GIF, validates the format and decoded transparency, then moves it to the requested path. Do not key out a black background automatically: that changes opaque source pixels and requires an explicit user request.
