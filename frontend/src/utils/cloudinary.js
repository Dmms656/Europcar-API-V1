const CLOUD_NAME = import.meta.env.VITE_CLOUDINARY_CLOUD_NAME;
const UPLOAD_PRESET = import.meta.env.VITE_CLOUDINARY_UPLOAD_PRESET;
const UPLOAD_URL = `https://api.cloudinary.com/v1_1/${CLOUD_NAME}/image/upload`;

/**
 * Sube una imagen a Cloudinary usando unsigned upload.
 * @param {File} file - Archivo de imagen desde input o drag&drop
 * @returns {Promise<{url: string, publicId: string}>} URL segura de la imagen
 */
export async function uploadToCloudinary(file) {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('upload_preset', UPLOAD_PRESET);
  formData.append('folder', 'europcar/vehiculos');

  const response = await fetch(UPLOAD_URL, {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error?.message || 'Error al subir imagen a Cloudinary');
  }

  const data = await response.json();
  return {
    url: data.secure_url,
    publicId: data.public_id,
  };
}
