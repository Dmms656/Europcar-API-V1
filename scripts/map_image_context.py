"""
Mapea cada imagen del documento original con el encabezado y parrafo cercano.

Estrategia: procesar el XML como texto, recorriendo cada bloque <w:p>...</w:p>
en orden. Por cada parrafo se extrae el texto, su estilo (Heading*) y, si lo
tiene, el o los r:embed referenciados.
"""

from __future__ import annotations

import json
import re
import zipfile
from pathlib import Path
from xml.etree import ElementTree as ET

REPO_ROOT = Path(__file__).resolve().parent.parent
SRC = REPO_ROOT / "Contexto" / "EUROPCAR.docx"
OUT_JSON = REPO_ROOT / "Contexto" / "imagenes_origen" / "captions.json"

PKG_REL_NS = "{http://schemas.openxmlformats.org/package/2006/relationships}"

P_BLOCK_RE = re.compile(r"<w:p[\s>].*?</w:p>", re.S)
PSTYLE_RE = re.compile(r'<w:pStyle\s+w:val="([^"]+)"')
EMBED_RE = re.compile(r'r:embed="([^"]+)"')
TEXT_RE = re.compile(r"<w:t[^>]*>(.*?)</w:t>", re.S)


def text_of(p_xml: str) -> str:
    return "".join(TEXT_RE.findall(p_xml)).strip()


def style_of(p_xml: str) -> str | None:
    m = PSTYLE_RE.search(p_xml)
    return m.group(1) if m else None


def main() -> None:
    with zipfile.ZipFile(SRC) as z:
        rels_root = ET.fromstring(
            z.read("word/_rels/document.xml.rels").decode("utf-8")
        )
        id_to_target: dict[str, str] = {}
        for rel in rels_root.findall(PKG_REL_NS + "Relationship"):
            if rel.get("Type", "").endswith("/image"):
                id_to_target[rel.get("Id")] = rel.get("Target")

        doc_xml = z.read("word/document.xml").decode("utf-8")

    captions: list[dict] = []
    current_heading = ""
    last_paragraph = ""
    seen: set[str] = set()
    image_index = 0

    for m in P_BLOCK_RE.finditer(doc_xml):
        p_xml = m.group(0)
        style = (style_of(p_xml) or "").lower()
        text = text_of(p_xml)
        is_heading = style.startswith("heading") or style.startswith("titulo") or style.startswith("ttulo")

        for rid in EMBED_RE.findall(p_xml):
            target = id_to_target.get(rid, rid)
            if target in seen:
                continue
            seen.add(target)
            image_index += 1
            captions.append(
                {
                    "index": image_index,
                    "style": style,
                    "heading": current_heading[:160],
                    "previous_paragraph": last_paragraph[:240],
                    "this_paragraph": text[:240],
                    "target": target,
                    "rid": rid,
                }
            )

        if is_heading and text:
            current_heading = text
        if text:
            last_paragraph = text

    OUT_JSON.parent.mkdir(parents=True, exist_ok=True)
    OUT_JSON.write_text(json.dumps(captions, ensure_ascii=False, indent=2), encoding="utf-8")
    for c in captions:
        print(f"{c['index']:02d} | {c['heading'][:80]} | {c['previous_paragraph'][:80]} | {c['target']}")


if __name__ == "__main__":
    main()
