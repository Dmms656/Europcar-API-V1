const CLOUD_NAME = import.meta.env.VITE_CLOUDINARY_CLOUD_NAME;
const UPLOAD_PRESET = import.meta.env.VITE_CLOUDINARY_UPLOAD_PRESET;

function assertConfig() {
  const missing = [];
  if (!CLOUD_NAME) missing.push('VITE_CLOUDINARY_CLOUD_NAME');
  if (!UPLOAD_PRESET) missing.push('VITE_CLOUDINARY_UPLOAD_PRESET');
  if (missing.length) {
    throw new Error(
      `Cloudinary no está configurado. Falta(n) ${missing.join(', ')} en frontend/.env. ` +
      `Copia frontend/.env.example a frontend/.env, completa los valores y reinicia el servidor de Vite.`
    );
  }
}

/**
 * Sube una imagen a Cloudinary usando unsigned upload.
 * @param {File} file - Archivo de imagen desde input o drag&drop
 * @returns {Promise<{url: string, publicId: string}>} URL segura de la imagen
 */
export async function uploadToCloudinary(file) {
  assertConfig();

  const uploadUrl = `https://api.cloudinary.com/v1_1/${CLOUD_NAME}/image/upload`;

  const formData = new FormData();
  formData.append('file', file);
  formData.append('upload_preset', UPLOAD_PRESET);
  formData.append('folder', 'europcar/vehiculos');

  const response = await fetch(uploadUrl, {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    let message = `Error al subir imagen a Cloudinary (HTTP ${response.status}).`;
    try {
      const error = await response.json();
      if (error?.error?.message) message = error.error.message;
    } catch {
      // body no JSON, conservar el mensaje generico con el HTTP status
    }
    throw new Error(message);
  }

  const data = await response.json();
  return {
    url: data.secure_url,
    publicId: data.public_id,
  };
}

export const cloudinaryConfig = {
  cloudName: CLOUD_NAME,
  uploadPreset: UPLOAD_PRESET,
  isConfigured: Boolean(CLOUD_NAME && UPLOAD_PRESET),
};
