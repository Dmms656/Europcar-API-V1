# -*- coding: utf-8 -*-
"""Capturas para el informe de laboratorio (Swagger, login, endpoints)."""
from pathlib import Path

from playwright.sync_api import sync_playwright

OUT = Path(r"c:\Users\medin\source\repos\Proyecto Desarrollo\Contexto\imagenes_laboratorio\informe")
BASE_LEGACY = "http://localhost:5207"
BASE_MW = "http://localhost:5200"


def shot(page, path: Path, full_page: bool = True) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    page.screenshot(path=str(path), full_page=full_page)
    print(f"OK {path.name}")


def capture_legacy(playwright) -> None:
    browser = playwright.chromium.launch()
    page = browser.new_page(viewport={"width": 1400, "height": 900})

    page.goto(f"{BASE_LEGACY}/swagger/index.html", wait_until="networkidle", timeout=60000)
    shot(page, OUT / "10_swagger_monolito_inicio.png")

    # Expandir Auth si existe
    try:
        page.get_by_text("Auth", exact=False).first.click(timeout=3000)
        page.wait_for_timeout(500)
    except Exception:
        pass
    shot(page, OUT / "11_swagger_monolito_auth.png")

    try:
        page.get_by_text("Booking", exact=False).first.click(timeout=3000)
        page.wait_for_timeout(500)
    except Exception:
        pass
    shot(page, OUT / "12_swagger_monolito_booking.png")

    # Login vía fetch y mostrar respuesta en página auxiliar
    page.goto(f"{BASE_LEGACY}/swagger/index.html", wait_until="networkidle")
    login_js = """
    async () => {
      const r = await fetch('/api/v1/Auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ username: 'admin.dev', password: '12345' })
      });
      const j = await r.json();
      return { status: r.status, body: j, hasTokenInBody: !!(j.data && j.data.token) };
    }
    """
    result = page.evaluate(login_js)
    html = f"""<!DOCTYPE html><html><head><meta charset='utf-8'>
    <title>Login seguro - laboratorio</title>
    <style>body{{font-family:Segoe UI,sans-serif;padding:24px;background:#f5f5f5}}
    pre{{background:#fff;border:1px solid #ccc;padding:16px;overflow:auto}}
    .ok{{color:#0a0}} .warn{{color:#a60}}</style></head><body>
    <h2>POST /api/v1/Auth/login (admin.dev)</h2>
    <p>HTTP <strong>{result['status']}</strong> —
    Token en JSON: <strong class="{'warn' if result['hasTokenInBody'] else 'ok'}">
    {'SÍ (riesgo)' if result['hasTokenInBody'] else 'NO — cookie HttpOnly rc_auth'}</strong></p>
    <p>Revisar DevTools → Application → Cookies → rc_auth</p>
    <pre>{result['body']}</pre></body></html>"""
    page.set_content(html)
    shot(page, OUT / "13_login_sin_token_en_json.png", full_page=False)

    # guest-client respuesta mínima
    guest_js = """
    async () => {
      const r = await fetch('/api/v1/reservas/guest-client', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          cedula: '9999999999',
          nombre: 'Prueba',
          apellido: 'Laboratorio',
          correo: 'lab@test.com',
          telefono: '0999999999'
        })
      });
      return { status: r.status, body: await r.json() };
    }
    """
    guest = page.evaluate(guest_js)
    keys = list(guest["body"].get("data", guest["body"]).keys()) if isinstance(guest.get("body"), dict) else []
    html2 = f"""<!DOCTYPE html><html><head><meta charset='utf-8'>
    <title>guest-client</title><style>body{{font-family:Segoe UI;padding:24px}}
    pre{{background:#fff;border:1px solid #ccc;padding:16px}}</style></head><body>
    <h2>POST /api/v1/reservas/guest-client</h2>
    <p>HTTP {guest['status']} — campos en data: <strong>{', '.join(keys) if keys else 'N/A'}</strong></p>
    <pre>{guest['body']}</pre></body></html>"""
    page.set_content(html2)
    shot(page, OUT / "14_guest_client_respuesta_minima.png", full_page=False)

    browser.close()


def capture_middleware(playwright) -> None:
    browser = playwright.chromium.launch()
    page = browser.new_page(viewport={"width": 1400, "height": 900})
    try:
        page.goto(f"{BASE_MW}/swagger/index.html", wait_until="networkidle", timeout=15000)
        shot(page, OUT / "15_swagger_middleware_v1_v2.png")
    except Exception as ex:
        print(f"Middleware no disponible: {ex}")
    browser.close()


def main() -> None:
    with sync_playwright() as p:
        capture_legacy(p)
        capture_middleware(p)
    print(f"\nCapturas en: {OUT}")


if __name__ == "__main__":
    main()
