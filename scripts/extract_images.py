"""
Extrae todas las imagenes embebidas en Contexto/EUROPCAR.docx,
en el orden en que aparecen en el documento, hacia Contexto/imagenes_origen/.
"""

from __future__ import annotations

import os
import re
import zipfile
from pathlib import Path
from xml.etree import ElementTree as ET


REPO_ROOT = Path(__file__).resolve().parent.parent
SRC = REPO_ROOT / "Contexto" / "EUROPCAR.docx"
OUT_DIR = REPO_ROOT / "Contexto" / "imagenes_origen"

PKG_REL_NS = "{http://schemas.openxmlformats.org/package/2006/relationships}"


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(SRC) as z:
        rels_root = ET.fromstring(z.read("word/_rels/document.xml.rels").decode("utf-8"))
        id_to_target: dict[str, str] = {}
        for rel in rels_root.findall(PKG_REL_NS + "Relationship"):
            if rel.get("Type", "").endswith("/image"):
                id_to_target[rel.get("Id")] = rel.get("Target")

        doc_xml = z.read("word/document.xml").decode("utf-8")
        embeds = re.findall(r'r:embed="([^"]+)"', doc_xml)

        ordered_targets: list[str] = []
        seen: set[str] = set()
        for rid in embeds:
            target = id_to_target.get(rid)
            if target and target not in seen:
                seen.add(target)
                ordered_targets.append(target)

        for i, target in enumerate(ordered_targets, start=1):
            arc = "word/" + target.lstrip("./")
            data = z.read(arc)
            ext = os.path.splitext(arc)[1].lower() or ".png"
            out_path = OUT_DIR / f"figura_{i:02d}{ext}"
            out_path.write_bytes(data)
            print(f"{i:02d}\t{target}\t{out_path.name}\t{len(data)} bytes")

    print(f"\nTotal: {len(ordered_targets)} imagenes -> {OUT_DIR}")


if __name__ == "__main__":
    main()
