import api from './axiosClient';

export type FacturaItem = {
  idFactura: number;
  numeroFactura?: string;
  fechaEmision?: string;
  codigoReserva?: string;
  numeroContrato?: string;
  subtotal?: number;
  valorIva?: number;
  total?: number;
  estadoFactura?: string;
};

export const facturasApi = {
  getMyFacturas: () => api.get('/Facturas/mis-facturas'),
};
