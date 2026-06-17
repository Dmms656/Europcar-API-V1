const CLOUD_NAME = process.env.EXPO_PUBLIC_CLOUDINARY_CLOUD_NAME;
const UPLOAD_PRESET = process.env.EXPO_PUBLIC_CLOUDINARY_UPLOAD_PRESET;

function assertConfig() {
  const missing: string[] = [];
  if (!CLOUD_NAME) missing.push('EXPO_PUBLIC_CLOUDINARY_CLOUD_NAME');
  if (!UPLOAD_PRESET) missing.push('EXPO_PUBLIC_CLOUDINARY_UPLOAD_PRESET');
  if (missing.length) {
    throw new Error(
      `Cloudinary no configurado. Falta(n) ${missing.join(', ')} en variables de entorno.`,
    );
  }
}

/** Sube imagen a Cloudinary (unsigned upload), carpeta europcar/vehiculos. */
export async function uploadToCloudinary(file: Blob | File): Promise<{ url: string; publicId: string }> {
  assertConfig();
  const uploadUrl = `https://api.cloudinary.com/v1_1/${CLOUD_NAME}/image/upload`;
  const formData = new FormData();
  formData.append('file', file as Blob);
  formData.append('upload_preset', UPLOAD_PRESET!);
  formData.append('folder', 'europcar/vehiculos');

  const response = await fetch(uploadUrl, { method: 'POST', body: formData });
  if (!response.ok) {
    let message = `Error Cloudinary (HTTP ${response.status})`;
    try {
      const err = await response.json();
      if (err?.error?.message) message = err.error.message;
    } catch {
      /* ignore */
    }
    throw new Error(message);
  }
  const data = await response.json();
  return { url: data.secure_url, publicId: data.public_id };
}

export const cloudinaryConfig = {
  cloudName: CLOUD_NAME,
  uploadPreset: UPLOAD_PRESET,
  isConfigured: Boolean(CLOUD_NAME && UPLOAD_PRESET),
};
