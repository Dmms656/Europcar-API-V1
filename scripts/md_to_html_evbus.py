#!/usr/bin/env python3
"""Convierte docs/Migracion-ESB-a-EventBus-RabbitMQ-Marketplace.md a HTML estilizado."""
from __future__ import annotations

import re
import sys
from pathlib import Path

try:
    import markdown
except ImportError:
    markdown = None  # type: ignore

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "docs" / "Migracion-ESB-a-EventBus-RabbitMQ-Marketplace.md"
DST = ROOT / "docs" / "Migracion-ESB-a-EventBus-RabbitMQ-Marketplace.html"

HTML_TEMPLATE = """<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Migración ESB → Event Bus (RabbitMQ) — RedCar Marketplace</title>
  <link rel="preconnect" href="https://fonts.googleapis.com" />
  <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
  <link href="https://fonts.googleapis.com/css2?family=DM+Sans:ital,opsz,wght@0,9..40,400;0,9..40,500;0,9..40,600;0,9..40,700;1,9..40,400&family=JetBrains+Mono:wght@400;500&display=swap" rel="stylesheet" />
  <style>
    :root {{
      --bg: #0f1419;
      --surface: #1a2332;
      --surface-2: #243044;
      --text: #e8edf4;
      --text-muted: #94a3b8;
      --accent: #f97316;
      --accent-soft: rgba(249, 115, 22, 0.15);
      --border: #334155;
      --link: #38bdf8;
      --success: #34d399;
      --warning: #fbbf24;
      --code-bg: #0d1117;
      --shadow: 0 4px 24px rgba(0, 0, 0, 0.35);
      --radius: 10px;
      --max-width: 920px;
    }}
    * {{ box-sizing: border-box; }}
    html {{ scroll-behavior: smooth; }}
    body {{
      margin: 0;
      font-family: "DM Sans", system-ui, sans-serif;
      font-size: 1.05rem;
      line-height: 1.65;
      color: var(--text);
      background: var(--bg);
      background-image:
        radial-gradient(ellipse 80% 50% at 50% -20%, rgba(249, 115, 22, 0.12), transparent),
        linear-gradient(180deg, var(--bg) 0%, #121820 100%);
    }}
    .layout {{
      display: grid;
      grid-template-columns: 280px 1fr;
      min-height: 100vh;
    }}
    @media (max-width: 1024px) {{
      .layout {{ grid-template-columns: 1fr; }}
      .sidebar {{ position: relative; height: auto; border-right: none; border-bottom: 1px solid var(--border); }}
    }}
    .sidebar {{
      position: sticky;
      top: 0;
      height: 100vh;
      overflow-y: auto;
      padding: 1.5rem 1.25rem 2rem;
      background: var(--surface);
      border-right: 1px solid var(--border);
    }}
    .sidebar-brand {{
      font-weight: 700;
      font-size: 0.85rem;
      letter-spacing: 0.04em;
      text-transform: uppercase;
      color: var(--accent);
      margin-bottom: 0.25rem;
    }}
    .sidebar-title {{
      font-size: 1rem;
      font-weight: 600;
      line-height: 1.35;
      margin: 0 0 1.25rem;
      color: var(--text);
    }}
    .sidebar nav ul {{
      list-style: none;
      padding: 0;
      margin: 0;
    }}
    .sidebar nav li {{ margin: 0.15rem 0; }}
    .sidebar nav a {{
      display: block;
      padding: 0.35rem 0.6rem;
      font-size: 0.88rem;
      color: var(--text-muted);
      text-decoration: none;
      border-radius: 6px;
      transition: color 0.15s, background 0.15s;
    }}
    .sidebar nav a:hover {{
      color: var(--text);
      background: var(--surface-2);
    }}
    .sidebar nav .toc-h3 {{ padding-left: 1rem; font-size: 0.82rem; }}
    main {{
      padding: 2.5rem 3rem 4rem;
      max-width: calc(var(--max-width) + 6rem);
    }}
    @media (max-width: 640px) {{
      main {{ padding: 1.5rem 1.25rem 3rem; }}
    }}
    .doc-header {{
      margin-bottom: 2.5rem;
      padding-bottom: 2rem;
      border-bottom: 1px solid var(--border);
    }}
    .doc-header h1 {{
      font-size: clamp(1.75rem, 4vw, 2.35rem);
      font-weight: 700;
      line-height: 1.2;
      margin: 0 0 0.5rem;
      background: linear-gradient(135deg, var(--text) 0%, #cbd5e1 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
    }}
    .doc-header .subtitle {{
      font-size: 1.15rem;
      color: var(--text-muted);
      margin: 0 0 1.25rem;
    }}
    .meta-grid {{
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: 0.75rem;
    }}
    .meta-card {{
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: var(--radius);
      padding: 0.85rem 1rem;
      font-size: 0.9rem;
    }}
    .meta-card strong {{
      display: block;
      font-size: 0.72rem;
      text-transform: uppercase;
      letter-spacing: 0.06em;
      color: var(--text-muted);
      margin-bottom: 0.25rem;
    }}
    .content h2 {{
      font-size: 1.55rem;
      font-weight: 700;
      margin: 2.75rem 0 1rem;
      padding-top: 0.5rem;
      color: var(--text);
      border-bottom: 2px solid var(--accent-soft);
    }}
    .content h2:first-child {{ margin-top: 0; }}
    .content h3 {{
      font-size: 1.2rem;
      font-weight: 600;
      margin: 2rem 0 0.75rem;
      color: #f1f5f9;
    }}
    .content h4 {{
      font-size: 1.05rem;
      font-weight: 600;
      margin: 1.5rem 0 0.5rem;
      color: var(--accent);
    }}
    .content p {{ margin: 0.85rem 0; }}
    .content ul, .content ol {{
      margin: 0.85rem 0;
      padding-left: 1.4rem;
    }}
    .content li {{ margin: 0.35rem 0; }}
    .content a {{
      color: var(--link);
      text-decoration: none;
      border-bottom: 1px solid transparent;
      transition: border-color 0.15s;
    }}
    .content a:hover {{ border-bottom-color: var(--link); }}
    .content strong {{ color: #f8fafc; font-weight: 600; }}
    .content em {{ color: var(--text-muted); }}
    .content hr {{
      border: none;
      height: 1px;
      background: var(--border);
      margin: 2.5rem 0;
    }}
    .content table {{
      width: 100%;
      border-collapse: collapse;
      margin: 1.25rem 0;
      font-size: 0.92rem;
      background: var(--surface);
      border-radius: var(--radius);
      overflow: hidden;
      box-shadow: var(--shadow);
    }}
    .content thead {{
      background: var(--surface-2);
    }}
    .content th, .content td {{
      padding: 0.65rem 0.9rem;
      text-align: left;
      border-bottom: 1px solid var(--border);
    }}
    .content th {{
      font-weight: 600;
      color: var(--accent);
      font-size: 0.8rem;
      text-transform: uppercase;
      letter-spacing: 0.03em;
    }}
    .content tr:last-child td {{ border-bottom: none; }}
    .content tr:hover td {{ background: rgba(255,255,255,0.02); }}
    .content code {{
      font-family: "JetBrains Mono", monospace;
      font-size: 0.88em;
      background: var(--code-bg);
      color: #fb923c;
      padding: 0.15em 0.45em;
      border-radius: 4px;
      border: 1px solid var(--border);
    }}
    .content pre {{
      background: var(--code-bg);
      border: 1px solid var(--border);
      border-radius: var(--radius);
      padding: 1.1rem 1.25rem;
      overflow-x: auto;
      margin: 1.25rem 0;
      box-shadow: var(--shadow);
    }}
    .content pre code {{
      background: none;
      border: none;
      padding: 0;
      color: #e2e8f0;
      font-size: 0.82rem;
      line-height: 1.5;
    }}
    .mermaid-wrap {{
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: var(--radius);
      padding: 1.5rem;
      margin: 1.5rem 0;
      overflow-x: auto;
      box-shadow: var(--shadow);
    }}
    .mermaid {{ text-align: center; }}
    .content blockquote {{
      margin: 1.25rem 0;
      padding: 1rem 1.25rem;
      border-left: 4px solid var(--accent);
      background: var(--accent-soft);
      border-radius: 0 var(--radius) var(--radius) 0;
      color: var(--text-muted);
    }}
    .badge-p0 {{ color: #fca5a5; }}
    .badge-p1 {{ color: #fcd34d; }}
    .footer-note {{
      margin-top: 3rem;
      padding-top: 1.5rem;
      border-top: 1px solid var(--border);
      font-size: 0.9rem;
      color: var(--text-muted);
      font-style: italic;
    }}
    .print-hint {{
      position: fixed;
      bottom: 1rem;
      right: 1rem;
      font-size: 0.75rem;
      color: var(--text-muted);
      opacity: 0.7;
    }}
    @media print {{
      body {{ background: white; color: #111; }}
      .sidebar, .print-hint {{ display: none; }}
      .layout {{ display: block; }}
      main {{ max-width: 100%; padding: 1rem; }}
      .content table, .mermaid-wrap {{ box-shadow: none; break-inside: avoid; }}
    }}
  </style>
</head>
<body>
  <div class="layout">
    <aside class="sidebar">
      <div class="sidebar-brand">RedCar / Europcar V2</div>
      <p class="sidebar-title">Migración ESB → Event Bus</p>
      <nav id="doc-toc">{toc_nav}</nav>
    </aside>
    <main>
      <header class="doc-header">
        <h1>Migración ESB → Event Bus (EvB) con RabbitMQ</h1>
        <p class="subtitle">Plataforma RedCar / Europcar — Canal Marketplace (Booking)</p>
        {meta_cards}
      </header>
      <article class="content">
        {body}
      </article>
      <p class="footer-note">{footer}</p>
    </main>
  </div>
  <p class="print-hint">Ctrl+P para imprimir / PDF</p>
  <script type="module">
    import mermaid from "https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs";
    mermaid.initialize({{
      startOnLoad: true,
      theme: "dark",
      themeVariables: {{
        primaryColor: "#1a2332",
        primaryTextColor: "#e8edf4",
        primaryBorderColor: "#f97316",
        lineColor: "#64748b",
        secondaryColor: "#243044",
        tertiaryColor: "#0f1419",
        fontFamily: "DM Sans, sans-serif"
      }},
      flowchart: {{ curve: "basis" }},
      sequence: {{ actorMargin: 50 }}
    }});
  </script>
</body>
</html>
"""


def slugify(text: str) -> str:
    text = text.lower().strip()
    text = re.sub(r"[^\w\s-]", "", text, flags=re.UNICODE)
    text = re.sub(r"[\s_]+", "-", text)
    return text.strip("-")


def preprocess_mermaid(md: str) -> str:
    def repl(m: re.Match) -> str:
        code = m.group(1).strip()
        return f'<div class="mermaid-wrap"><pre class="mermaid">\n{code}\n</pre></div>\n'
    return re.sub(r"```mermaid\s*\n(.*?)```", repl, md, flags=re.DOTALL)


def preprocess_meta_table(md: str) -> tuple[str, str]:
    """Extrae tabla de metadatos inicial y genera cards HTML."""
    pattern = r"^\| Metadato \| Valor \|\n\|[-| ]+\|\n((?:\|[^\n]+\|\n)+)"
    m = re.search(pattern, md, re.MULTILINE)
    if not m:
        return md, ""
    rows = []
    for line in m.group(1).strip().split("\n"):
        parts = [c.strip() for c in line.strip("|").split("|")]
        if len(parts) >= 2:
            key = re.sub(r"\*\*", "", parts[0])
            val = parts[1]
            val = inline_format(val)
            rows.append((key, val))
    cards = '<div class="meta-grid">' + "".join(
        f'<div class="meta-card"><strong>{k}</strong>{v}</div>' for k, v in rows
    ) + "</div>"
    md = md[: m.start()] + md[m.end() :]
    return md, cards


def build_toc_nav(md: str) -> str:
    items = []
    for line in md.splitlines():
        if line.startswith("## ") and not line.startswith("## Tabla"):
            title = line[3:].strip()
            sid = slugify(title)
            items.append(f'<li><a href="#{sid}">{title}</a></li>')
        elif line.startswith("### "):
            title = line[4:].strip()
            sid = slugify(title)
            items.append(f'<li class="toc-h3"><a href="#{sid}">{title}</a></li>')
    return "<ul>" + "\n".join(items) + "</ul>" if items else ""


def add_heading_ids(html: str) -> str:
    def repl(m: re.Match) -> str:
        level, title = m.group(1), m.group(2)
        sid = slugify(re.sub(r"<[^>]+>", "", title))
        return f'<h{level} id="{sid}">{title}</h{level}>'

    html = re.sub(
        r"<h([234])>(.*?)</h\1>",
        repl,
        html,
        flags=re.DOTALL,
    )
    return html


def simple_md_to_html(md: str) -> str:
    """Fallback mínimo si no hay librería markdown."""
    lines = md.splitlines()
    out: list[str] = []
    in_pre = False
    in_table = False
    table_rows: list[str] = []

    def flush_table():
        nonlocal in_table, table_rows
        if not table_rows:
            return
        html_rows = []
        for i, row in enumerate(table_rows):
            cells = [c.strip() for c in row.strip("|").split("|")]
            tag = "th" if i == 0 else "td"
            html_rows.append("<tr>" + "".join(f"<{tag}>{c}</{tag}>" for c in cells) + "</tr>")
        out.append("<table><thead>" + html_rows[0] + "</thead><tbody>" + "".join(html_rows[1:]) + "</tbody></table>")
        table_rows = []
        in_table = False

    for line in lines:
        if line.strip().startswith("```") and not in_pre:
            if "mermaid" in line:
                continue
            in_pre = True
            lang = line.strip("`").strip() or ""
            out.append(f'<pre><code class="language-{lang}">')
            continue
        if in_pre:
            if line.strip() == "```":
                in_pre = False
                out.append("</code></pre>")
            else:
                out.append(line.replace("<", "&lt;").replace(">", "&gt;"))
            continue
        if line.startswith("|"):
            in_table = True
            if re.match(r"^\|[\s\-:|]+\|$", line):
                continue
            table_rows.append(line)
            continue
        if in_table:
            flush_table()
        if line.startswith("## "):
            out.append(f"<h2>{line[3:]}</h2>")
        elif line.startswith("### "):
            out.append(f"<h4>{line[4:]}</h4>")
        elif line.startswith("#### "):
            out.append(f"<h4>{line[5:]}</h4>")
        elif line.strip() == "---":
            out.append("<hr/>")
        elif line.startswith("- "):
            if not out or not out[-1].startswith("<ul"):
                out.append("<ul>")
            out.append(f"<li>{inline_format(line[2:])}</li>")
        elif re.match(r"^\d+\.\s", line):
            if not out or not out[-1].startswith("<ol"):
                out.append("<ol>")
            out.append(f"<li>{inline_format(re.sub(r'^\\d+\\.\\s', '', line))}</li>")
        elif line.strip().startswith(">"):
            out.append(f"<blockquote><p>{inline_format(line.lstrip('> '))}</p></blockquote>")
        elif line.strip() == "":
            if out and out[-1].startswith("<li>"):
                out.append("</ul>" if "<ul>" in "".join(out[-5:]) else "</ol>")
            continue
        elif line.strip():
            out.append(f"<p>{inline_format(line)}</p>")
    flush_table()
    return "\n".join(out)


def inline_format(text: str) -> str:
    text = re.sub(r"\*\*([^*]+)\*\*", r"<strong>\1</strong>", text)
    text = re.sub(r"\*([^*]+)\*", r"<em>\1</em>", text)
    text = re.sub(r"`([^`]+)`", r"<code>\1</code>", text)
    text = re.sub(r"\[([^\]]+)\]\(([^)]+)\)", r'<a href="\2">\1</a>', text)
    return text


def md_to_html(md: str) -> str:
    md = preprocess_mermaid(md)
    md, meta_cards = preprocess_meta_table(md)
    # Quitar título duplicado (va en header)
    md = re.sub(r"^# Migración.*?\n## Plataforma.*?\n\n", "", md, count=1, flags=re.DOTALL)
    md = re.sub(r"^## Tabla de contenidos.*?(?=^## 1\.)", "", md, count=1, flags=re.DOTALL | re.MULTILINE)

    toc_nav = build_toc_nav(md)
    footer = ""
    if "*Documento generado" in md:
        parts = md.rsplit("---\n\n", 1)
        if len(parts) == 2 and parts[1].strip().startswith("*"):
            md, footer = parts[0], parts[1].strip().strip("*")

    if markdown:
        body = markdown.markdown(
            md,
            extensions=["tables", "fenced_code", "nl2br", "sane_lists"],
        )
    else:
        body = simple_md_to_html(md)

    body = add_heading_ids(body)
    footer_html = inline_format(footer) if footer else ""

    return HTML_TEMPLATE.format(
        toc_nav=toc_nav,
        meta_cards=meta_cards,
        body=body,
        footer=footer_html,
    )


def main() -> int:
    if not SRC.exists():
        print(f"No encontrado: {SRC}", file=sys.stderr)
        return 1
    md = SRC.read_text(encoding="utf-8")
    html = md_to_html(md)
    DST.write_text(html, encoding="utf-8")
    print(f"Generado: {DST}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
