import { useState, useEffect } from 'react';
import { useAuthStore } from '../../store/useAuthStore';
import { vehiculosApi } from '../../api/vehiculosApi';
import { clientesApi } from '../../api/clientesApi';
import { contratosApi } from '../../api/contratosApi';
import { Car, Users, CalendarCheck, FileText, Loader2, RefreshCw, TrendingUp } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function DashboardPage() {
  const { user } = useAuthStore();
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => { loadStats(); }, []);

  const loadStats = async () => {
    setLoading(true);
    try {
      const [vRes, cRes, ctRes] = await Promise.all([
        vehiculosApi.getAll(),
        clientesApi.getAll(),
        contratosApi.getAll(),
      ]);
      const vehiculos = vRes.data?.data || [];
      const clientes = cRes.data?.data || [];
      const contratos = ctRes.data?.data || [];

      setStats({
        totalVehiculos: vehiculos.length,
        vehiculosDisponibles: vehiculos.filter(v => v.estadoOperativo === 'DISPONIBLE').length,
        vehiculosAlquilados: vehiculos.filter(v => v.estadoOperativo === 'ALQUILADO').length,
        totalClientes: clientes.length,
        totalContratos: contratos.length,
        contratosAbiertos: contratos.filter(c => c.estadoContrato === 'ABIERTO').length,
      });
    } catch (e) {
      setStats({
        totalVehiculos: '-', vehiculosDisponibles: '-', vehiculosAlquilados: '-',
        totalClientes: '-', totalContratos: '-', contratosAbiertos: '-',
      });
    } finally { setLoading(false); }
  };

  const cards = [
    { title: 'Vehículos', value: stats?.totalVehiculos, sub: `${stats?.vehiculosDisponibles} disponibles`, icon: Car, path: '/vehiculos', color: 'var(--color-primary)' },
    { title: 'Clientes', value: stats?.totalClientes, sub: 'registrados', icon: Users, path: '/clientes', color: 'var(--color-success)' },
    { title: 'Contratos', value: stats?.totalContratos, sub: `${stats?.contratosAbiertos} abiertos`, icon: FileText, path: '/contratos', color: 'var(--color-info)' },
    { title: 'Alquilados', value: stats?.vehiculosAlquilados, sub: 'vehículos en uso', icon: TrendingUp, path: '/reservas', color: 'var(--color-warning)' },
  ];

  return (
    <div className="dashboard">
      <div className="dashboard__welcome">
        <div>
          <h1>Bienvenido, {user?.username}</h1>
          <p>Panel de administración de Europcar Rental</p>
        </div>
        <button className="btn btn--outline btn--sm" onClick={loadStats} disabled={loading}>
          <RefreshCw size={16} className={loading ? 'spin' : ''} /> Actualizar
        </button>
      </div>

      {loading ? (
        <div className="module-loading"><Loader2 size={24} className="spin" /> Cargando estadísticas...</div>
      ) : (
        <div className="dashboard__cards">
          {cards.map((card) => (
            <Link key={card.path} to={card.path} className="dashboard-card">
              <div className="dashboard-card__icon" style={{ background: card.color }}>
                <card.icon size={24} color="white" />
              </div>
              <div className="dashboard-card__content">
                <span className="dashboard-card__value">{card.value}</span>
                <h3>{card.title}</h3>
                <p>{card.sub}</p>
              </div>
            </Link>
          ))}
        </div>
      )}

      <div className="dashboard__info">
        <div className="info-card">
          <h3>Información de sesión</h3>
          <div className="info-card__row">
            <span>Usuario</span>
            <strong>{user?.username}</strong>
          </div>
          <div className="info-card__row">
            <span>Correo</span>
            <strong>{user?.correo}</strong>
          </div>
          <div className="info-card__row">
            <span>Roles</span>
            <div className="info-card__badges">
              {user?.roles?.map((role) => (
                <span key={role} className="badge">{role}</span>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
