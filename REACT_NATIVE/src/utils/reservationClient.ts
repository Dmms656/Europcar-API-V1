import { clientesApi } from '@/src/api/clientesApi';
import type { UserProfile } from '@/src/store/useAuthStore';
import { unwrapData } from '@/src/utils/apiResponse';
import { mergeUserProfile } from '@/src/utils/authProfile';
import { clienteDtoToGuestForm, guestFormFromUserProfile } from '@/src/utils/bookingNormalize';

export type ReservationGuestForm = {
  nombre: string;
  apellido: string;
  cedula: string;
  correo: string;
  telefono: string;
  direccion: string;
};

/** Carga cédula/nombres desde MS Clientes cuando /Auth/me no los trae. */
export async function loadReservationClientData(user: UserProfile | null): Promise<{
  guestForm: ReservationGuestForm | null;
  user: UserProfile | null;
}> {
  if (!user) return { guestForm: null, user: null };

  let mergedUser = user;
  let guestForm = guestFormFromUserProfile(user);

  const needsClienteFetch =
    Boolean(user.idCliente) &&
    (!guestForm?.cedula || !guestForm?.nombre);

  if (needsClienteFetch && user.idCliente) {
    try {
      const res = await clientesApi.getById(user.idCliente);
      const dto = unwrapData<Record<string, unknown>>(res);
      const fromCliente = clienteDtoToGuestForm(dto);
      if (fromCliente) {
        guestForm = { ...(guestForm ?? emptyGuestForm()), ...fromCliente };
        mergedUser =
          mergeUserProfile(user, {
            nombres: fromCliente.nombre,
            apellidos: fromCliente.apellido,
            numeroIdentificacion: fromCliente.cedula,
            telefono: fromCliente.telefono,
            correo: fromCliente.correo,
            nombreCompleto: [fromCliente.nombre, fromCliente.apellido].filter(Boolean).join(' ').trim() || user.nombreCompleto,
          }) ?? user;
      }
    } catch {
      /* usar lo que haya en sesión */
    }
  }

  return { guestForm, user: mergedUser };
}

function emptyGuestForm(): ReservationGuestForm {
  return { nombre: '', apellido: '', cedula: '', correo: '', telefono: '', direccion: '' };
}

export function principalDisplayName(
  guestForm: ReservationGuestForm,
  user: UserProfile | null,
): string {
  const fromForm = [guestForm.nombre, guestForm.apellido].filter(Boolean).join(' ').trim();
  return fromForm || user?.nombreCompleto || user?.username || 'Cliente';
}
