"""
Genera diagramas de secuencia (PNG) para las 5 actividades principales del
sistema EUROPCAR V1.

Salida: Contexto/diagramas_secuencia/seq_<n>_<slug>.png
"""

from __future__ import annotations

import os
import re
from dataclasses import dataclass
from pathlib import Path
from typing import Literal

import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch, Rectangle


REPO_ROOT = Path(__file__).resolve().parent.parent
OUT_DIR = REPO_ROOT / "Contexto" / "diagramas_secuencia"


# ---------------------------------------------------------------------------
# Estilos
# ---------------------------------------------------------------------------

HEADER_COLOR = "#0F4C81"
HEADER_TEXT = "#FFFFFF"
LIFELINE_COLOR = "#9AA8B6"
ACTIVATION_FILL = "#DCE7F2"
ACTIVATION_EDGE = "#0F4C81"
CALL_COLOR = "#1B2631"
RETURN_COLOR = "#7B8A99"
NOTE_FILL = "#FFF8DC"
NOTE_EDGE = "#C9B458"
TITLE_COLOR = "#0F4C81"


# ---------------------------------------------------------------------------
# Modelo
# ---------------------------------------------------------------------------

@dataclass
class Message:
    src: str
    dst: str
    label: str
    kind: Literal["call", "return", "self"] = "call"
    note_after: str | None = None  # nota debajo del mensaje (opcional)


@dataclass
class SequenceDiagram:
    title: str
    actors: list[str]
    messages: list[Message]


# ---------------------------------------------------------------------------
# Render
# ---------------------------------------------------------------------------

def _slugify(text: str) -> str:
    text = re.sub(r"[^\w\s-]", "", text, flags=re.UNICODE).strip().lower()
    return re.sub(r"[-\s]+", "_", text)


def render(diagram: SequenceDiagram, out_path: Path) -> Path:
    """Dibuja el diagrama de secuencia y lo guarda como PNG."""
    n_actors = len(diagram.actors)
    n_msgs = len(diagram.messages)

    actor_step = 3.4
    width = max(11.0, 1.6 + actor_step * n_actors)
    msg_step = 0.95
    top_pad = 1.5
    bottom_pad = 1.0
    height = top_pad + bottom_pad + max(4.5, msg_step * (n_msgs + 1))

    fig, ax = plt.subplots(figsize=(width, height), dpi=180)
    ax.set_xlim(0, width)
    ax.set_ylim(0, height)
    ax.axis("off")

    ax.text(
        width / 2, height - 0.35, diagram.title,
        ha="center", va="center",
        fontsize=14, fontweight="bold", color=TITLE_COLOR,
    )

    actor_x = {
        name: 1.0 + actor_step / 2 + i * actor_step
        for i, name in enumerate(diagram.actors)
    }

    header_y = height - 1.05
    header_h = 0.55
    for name, x in actor_x.items():
        box = FancyBboxPatch(
            (x - actor_step / 2 + 0.2, header_y - header_h / 2),
            actor_step - 0.4, header_h,
            boxstyle="round,pad=0.02,rounding_size=0.08",
            linewidth=1.0, edgecolor=HEADER_COLOR, facecolor=HEADER_COLOR,
        )
        ax.add_patch(box)
        ax.text(
            x, header_y, name,
            ha="center", va="center",
            fontsize=10, fontweight="bold", color=HEADER_TEXT,
        )

    lifeline_top = header_y - header_h / 2 - 0.05
    lifeline_bottom = bottom_pad - 0.2
    for x in actor_x.values():
        ax.plot(
            [x, x], [lifeline_top, lifeline_bottom],
            color=LIFELINE_COLOR, linestyle=(0, (4, 4)), linewidth=1.0,
        )

    available_top = lifeline_top - 0.6
    available_bottom = lifeline_bottom + 0.3
    span = available_top - available_bottom
    step = span / max(n_msgs, 1)

    for i, msg in enumerate(diagram.messages):
        if msg.src not in actor_x or msg.dst not in actor_x:
            continue
        y = available_top - (i + 0.5) * step

        x1 = actor_x[msg.src]
        x2 = actor_x[msg.dst]
        is_self = msg.kind == "self" or msg.src == msg.dst
        is_return = msg.kind == "return"

        ax.add_patch(Rectangle(
            (min(x1, x2) - 0.06, y - step * 0.45),
            abs(x2 - x1) if not is_self else 0.0,
            step * 0.9,
            linewidth=0,
            facecolor="none",
        ))

        for x in (x1, x2):
            ax.add_patch(Rectangle(
                (x - 0.07, y - step * 0.45),
                0.14, step * 0.9,
                linewidth=0.6,
                edgecolor=ACTIVATION_EDGE,
                facecolor=ACTIVATION_FILL,
                alpha=0.95,
            ))

        color = RETURN_COLOR if is_return else CALL_COLOR
        ls = "--" if is_return else "-"
        arrow_style = "->" if not is_return else "->"

        if is_self:
            x = x1
            offset = 1.0
            ax.annotate(
                "",
                xy=(x + 0.07, y - step * 0.25),
                xytext=(x + offset, y - step * 0.25),
                arrowprops=dict(arrowstyle=arrow_style, color=color,
                                lw=1.4, linestyle=ls,
                                connectionstyle="arc3,rad=0.0"),
            )
            ax.plot([x + 0.07, x + offset], [y, y], color=color,
                    linestyle=ls, linewidth=1.4)
            ax.plot([x + offset, x + offset], [y, y - step * 0.25],
                    color=color, linestyle=ls, linewidth=1.4)
            ax.text(x + offset + 0.1, y + 0.03, msg.label,
                    ha="left", va="bottom", fontsize=9, color=color)
        else:
            sign = 1 if x2 > x1 else -1
            ax.annotate(
                "",
                xy=(x2 - sign * 0.08, y),
                xytext=(x1 + sign * 0.08, y),
                arrowprops=dict(arrowstyle=arrow_style, color=color,
                                lw=1.4, linestyle=ls),
            )
            label = msg.label
            mid_x = (x1 + x2) / 2
            ax.text(mid_x, y + 0.06, label,
                    ha="center", va="bottom",
                    fontsize=9, color=color, wrap=True)

        if msg.note_after:
            note_y = y - step * 0.55
            note_x = (x1 + x2) / 2 if not is_self else x1 + 1.4
            note = FancyBboxPatch(
                (note_x - 1.6, note_y - 0.18),
                3.2, 0.36,
                boxstyle="round,pad=0.02,rounding_size=0.06",
                linewidth=0.8, edgecolor=NOTE_EDGE, facecolor=NOTE_FILL,
            )
            ax.add_patch(note)
            ax.text(note_x, note_y, msg.note_after,
                    ha="center", va="center", fontsize=8, color="#5C4A0F",
                    style="italic")

    out_path.parent.mkdir(parents=True, exist_ok=True)
    plt.tight_layout()
    plt.savefig(out_path, dpi=180, bbox_inches="tight", facecolor="white")
    plt.close(fig)
    return out_path


# ---------------------------------------------------------------------------
# Definicion de los 5 diagramas
# ---------------------------------------------------------------------------

DIAGRAMS: list[SequenceDiagram] = [
    SequenceDiagram(
        title="CU-01 Iniciar sesion",
        actors=["Usuario", "Frontend (React)", "API (AuthController)", "AuthService", "DB / security"],
        messages=[
            Message("Usuario", "Frontend (React)", "Ingresa username y password"),
            Message("Frontend (React)", "API (AuthController)", "POST /api/v1/Auth/login"),
            Message("API (AuthController)", "AuthService", "LoginAsync(request)"),
            Message("AuthService", "DB / security", "SELECT usuarios_app por username"),
            Message("DB / security", "AuthService", "Usuario + roles", kind="return"),
            Message("AuthService", "AuthService", "Validar hash + intentos", kind="self"),
            Message("AuthService", "DB / security", "INSERT aud_intentos_login"),
            Message("AuthService", "API (AuthController)", "{ token, roles, usuario }", kind="return"),
            Message("API (AuthController)", "Frontend (React)", "200 OK ApiResponse", kind="return"),
            Message("Frontend (React)", "Frontend (React)", "useAuthStore.login() y redirige", kind="self"),
            Message("Frontend (React)", "Usuario", "Acceso al panel correspondiente", kind="return"),
        ],
    ),

    SequenceDiagram(
        title="CU-03 Buscar y reservar vehiculo",
        actors=["Cliente", "Frontend", "API Booking", "ReservaService", "DB / rental"],
        messages=[
            Message("Cliente", "Frontend", "Selecciona fechas y localizacion"),
            Message("Frontend", "API Booking", "GET /api/v1/vehiculos?filtros"),
            Message("API Booking", "DB / rental", "SELECT vehiculos disponibles"),
            Message("DB / rental", "API Booking", "Lista paginada", kind="return"),
            Message("API Booking", "Frontend", "200 OK con vehiculos", kind="return"),
            Message("Cliente", "Frontend", "Selecciona vehiculo y abre /reservar/:id"),
            Message("Frontend", "API Booking", "GET /api/v1/vehiculos/{id}/disponibilidad"),
            Message("API Booking", "Frontend", "Disponibilidad confirmada", kind="return"),
            Message("Frontend", "API Booking", "POST /api/v1/admin/reservas/guest-client (si invitado)"),
            Message("API Booking", "DB / rental", "UPSERT cliente"),
            Message("API Booking", "Frontend", "Cliente listo", kind="return"),
            Message("Frontend", "API Booking", "POST /api/v1/admin/reservas"),
            Message("API Booking", "ReservaService", "CreateAsync(request)"),
            Message("ReservaService", "ReservaService", "Validar fechas, hold, lead time", kind="self"),
            Message("ReservaService", "DB / rental", "INSERT reservas + extras"),
            Message("DB / rental", "ReservaService", "ReservaCreada", kind="return"),
            Message("ReservaService", "API Booking", "ReservaResponse", kind="return"),
            Message("API Booking", "Frontend", "201 Created (codigoReserva)", kind="return"),
            Message("Frontend", "Cliente", "Resumen de reserva PENDIENTE", kind="return"),
        ],
    ),

    SequenceDiagram(
        title="CU-04 Confirmar reserva con pago y factura",
        actors=["Agente POS", "Frontend", "API ReservasAdmin", "ReservaService", "DB / rental"],
        messages=[
            Message("Agente POS", "Frontend", "Abre reserva PENDIENTE"),
            Message("Frontend", "API ReservasAdmin", "PUT /api/v1/admin/reservas/{id}/confirmar"),
            Message("API ReservasAdmin", "ReservaService", "ConfirmarAsync(id, monto, ref)"),
            Message("ReservaService", "DB / rental", "UPDATE reserva = CONFIRMADA"),
            Message("ReservaService", "DB / rental", "INSERT pago"),
            Message("ReservaService", "DB / rental", "INSERT factura"),
            Message("DB / rental", "ReservaService", "Confirmacion atomica", kind="return"),
            Message("ReservaService", "API ReservasAdmin", "ReservaResponse", kind="return"),
            Message("API ReservasAdmin", "Frontend", "200 OK con codigo, pago y factura", kind="return"),
            Message("Frontend", "Agente POS", "Comprobante visible", kind="return"),
        ],
    ),

    SequenceDiagram(
        title="CU-06 Apertura de contrato (Check-out)",
        actors=["Agente POS", "Frontend", "API Contratos", "ContratoService", "DB / rental"],
        messages=[
            Message("Agente POS", "Frontend", "Selecciona reserva CONFIRMADA"),
            Message("Frontend", "API Contratos", "POST /api/v1/Contratos"),
            Message("API Contratos", "ContratoService", "CrearDesdeReservaAsync(request, usuario)"),
            Message("ContratoService", "DB / rental", "INSERT contratos (ABIERTO)"),
            Message("DB / rental", "ContratoService", "ContratoCreado", kind="return"),
            Message("Frontend", "API Contratos", "POST /api/v1/Contratos/checkout"),
            Message("API Contratos", "ContratoService", "RegistrarCheckOutAsync(...)"),
            Message("ContratoService", "DB / rental", "INSERT check_in_out (OUT)"),
            Message("ContratoService", "DB / rental", "UPDATE vehiculo = ALQUILADO"),
            Message("DB / rental", "ContratoService", "OK", kind="return"),
            Message("ContratoService", "API Contratos", "ContratoResponse", kind="return"),
            Message("API Contratos", "Frontend", "200 OK con contrato + vehiculo", kind="return"),
            Message("Frontend", "Agente POS", "Vehiculo entregado", kind="return"),
        ],
    ),

    SequenceDiagram(
        title="CU-07 Devolucion (Check-in)",
        actors=["Agente POS", "Frontend", "API Contratos", "ContratoService", "DB / rental"],
        messages=[
            Message("Agente POS", "Frontend", "Registra retorno (km, combustible, limpieza)"),
            Message("Frontend", "API Contratos", "POST /api/v1/Contratos/checkin"),
            Message("API Contratos", "ContratoService", "RegistrarCheckInAsync(...)"),
            Message("ContratoService", "ContratoService", "Calcular cargos (km, combustible, limpieza)", kind="self"),
            Message("ContratoService", "DB / rental", "INSERT check_in_out (IN)"),
            Message("ContratoService", "DB / rental", "UPDATE contrato = CERRADO"),
            Message("ContratoService", "DB / rental", "UPDATE vehiculo = DISPONIBLE"),
            Message("ContratoService", "DB / rental", "INSERT pago(s) por cargos extra (si aplica)"),
            Message("DB / rental", "ContratoService", "OK", kind="return"),
            Message("ContratoService", "API Contratos", "ContratoResponse", kind="return"),
            Message("API Contratos", "Frontend", "200 OK con totales y devolucion", kind="return"),
            Message("Frontend", "Agente POS", "Cierre de renta", kind="return"),
        ],
    ),
]


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    for i, d in enumerate(DIAGRAMS, start=1):
        slug = _slugify(d.title)
        out = OUT_DIR / f"seq_{i:02d}_{slug}.png"
        render(d, out)
        print(f"OK  {out.relative_to(REPO_ROOT)}")


if __name__ == "__main__":
    main()
