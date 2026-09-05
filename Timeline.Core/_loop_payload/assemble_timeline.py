#!/usr/bin/env python3
import base64, pathlib
parts = sorted(pathlib.Path(__file__).parent.glob("Timeline.cs.b64.*"))
data = "".join(p.read_text() for p in parts)
out = pathlib.Path(__file__).parent.parent / "Timeline.cs"
out.write_bytes(base64.b64decode(data))
print("Wrote", out, out.stat().st_size)
