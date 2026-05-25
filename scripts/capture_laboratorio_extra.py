from pathlib import Path
from playwright.sync_api import sync_playwright

OUT = Path(r"c:\Users\medin\source\repos\Proyecto Desarrollo\Contexto\imagenes_laboratorio\informe")
BASE = "http://localhost:5207"

with sync_playwright() as p:
    browser = p.chromium.launch()
    page = browser.new_page(viewport={"width": 1400, "height": 900})
    page.goto(f"{BASE}/swagger/index.html", wait_until="networkidle")
    page.evaluate(
        """async () => {
      await fetch('/api/v1/Auth/login', {
        method: 'POST', headers: {'Content-Type': 'application/json'},
        credentials: 'include',
        body: JSON.stringify({username:'admin.dev', password:'12345'})
      });
    }"""
    )
    cookies = page.context.cookies()
    rc = next((c for c in cookies if c["name"] == "rc_auth"), None)
    admin = page.evaluate(
        """async () => {
      const r = await fetch('/api/v1/admin/Vehiculos/disponibles');
      return {status: r.status, statusText: r.statusText};
    }"""
    )
    rc_row = (
        f"<tr><td>{rc['name']}</td><td>{rc.get('httpOnly')}</td>"
        f"<td>{rc.get('secure')}</td><td>{rc.get('sameSite')}</td></tr>"
        if rc
        else "<tr><td colspan=4>No cookie rc_auth</td></tr>"
    )
    html = f"""<!DOCTYPE html><html><head><meta charset=utf-8><style>
    body{{font-family:Segoe UI;padding:24px}} table{{border-collapse:collapse}}
    td,th{{border:1px solid #ccc;padding:8px}} .ok{{color:#060}}</style></head><body>
    <h2>Validación post-corrección (laboratorio)</h2>
    <h3>Cookie HttpOnly rc_auth tras login</h3>
    <table><tr><th>Nombre</th><th>HttpOnly</th><th>Secure</th><th>SameSite</th></tr>
    {rc_row}</table>
    <h3>GET /api/v1/admin/Vehiculos/disponibles sin Bearer</h3>
    <p class=ok>HTTP <strong>{admin['status']}</strong> {admin['statusText']}</p>
    </body></html>"""
    page.set_content(html)
    page.screenshot(path=str(OUT / "16_validacion_cookie_y_401_admin.png"))
    print("OK 16_validacion_cookie_y_401_admin.png")
    browser.close()
