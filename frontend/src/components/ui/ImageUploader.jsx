import { useState, useRef, useCallback } from 'react';
import { uploadToCloudinary } from '../../utils/cloudinary';
import { Upload, X, Image, Loader2, CheckCircle, AlertCircle } from 'lucide-react';

/**
 * Componente visual de subida de imágenes con:
 * - Drag & drop
 * - Click para seleccionar archivo
 * - Preview de la imagen
 * - Progreso de subida
 * - Integración con Cloudinary
 *
 * @param {Object} props
 * @param {string} props.value - URL actual de la imagen
 * @param {function} props.onChange - Callback con la URL de Cloudinary una vez subida
 * @param {string} props.label - Etiqueta del campo
 */
export default function ImageUploader({ value, onChange, label = 'Imagen del vehículo' }) {
  const [dragActive, setDragActive] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [preview, setPreview] = useState(value || '');
  const [error, setError] = useState('');
  const [uploadSuccess, setUploadSuccess] = useState(false);
  const inputRef = useRef(null);

  const handleFile = useCallback(async (file) => {
    if (!file) return;

    // Validar tipo
    if (!file.type.startsWith('image/')) {
      setError('Solo se permiten archivos de imagen (JPG, PNG, WebP)');
      return;
    }

    // Validar tamaño (máx 10MB)
    if (file.size > 10 * 1024 * 1024) {
      setError('La imagen no debe superar los 10MB');
      return;
    }

    setError('');
    setUploadSuccess(false);

    // Mostrar preview local inmediato
    const reader = new FileReader();
    reader.onload = (e) => setPreview(e.target.result);
    reader.readAsDataURL(file);

    // Subir a Cloudinary
    setUploading(true);
    try {
      const result = await uploadToCloudinary(file);
      setPreview(result.url);
      onChange(result.url);
      setUploadSuccess(true);
      setTimeout(() => setUploadSuccess(false), 3000);
    } catch (err) {
      setError(err.message || 'Error al subir la imagen');
      setPreview('');
    } finally {
      setUploading(false);
    }
  }, [onChange]);

  const handleDrag = useCallback((e) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true);
    } else if (e.type === 'dragleave') {
      setDragActive(false);
    }
  }, []);

  const handleDrop = useCallback((e) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);
    const file = e.dataTransfer.files?.[0];
    handleFile(file);
  }, [handleFile]);

  const handleInputChange = (e) => {
    const file = e.target.files?.[0];
    handleFile(file);
  };

  const removeImage = () => {
    setPreview('');
    onChange('');
    setError('');
    setUploadSuccess(false);
    if (inputRef.current) inputRef.current.value = '';
  };

  return (
    <div className="image-uploader">
      <label className="form-label">{label}</label>

      {/* Preview o Dropzone */}
      {preview ? (
        <div className="image-uploader__preview">
          <img src={preview} alt="Preview del vehículo" />
          <div className="image-uploader__preview-overlay">
            {uploading ? (
              <div className="image-uploader__status image-uploader__status--loading">
                <Loader2 size={24} className="spin" />
                <span>Subiendo a Cloudinary...</span>
              </div>
            ) : uploadSuccess ? (
              <div className="image-uploader__status image-uploader__status--success">
                <CheckCircle size={24} />
                <span>¡Imagen subida!</span>
              </div>
            ) : (
              <div className="image-uploader__preview-actions">
                <button
                  type="button"
                  className="btn btn--ghost btn--sm"
                  onClick={() => inputRef.current?.click()}
                >
                  Cambiar imagen
                </button>
                <button
                  type="button"
                  className="icon-btn icon-btn--danger"
                  onClick={removeImage}
                >
                  <X size={16} />
                </button>
              </div>
            )}
          </div>
        </div>
      ) : (
        <div
          className={`image-uploader__dropzone ${dragActive ? 'image-uploader__dropzone--active' : ''}`}
          onDragEnter={handleDrag}
          onDragLeave={handleDrag}
          onDragOver={handleDrag}
          onDrop={handleDrop}
          onClick={() => inputRef.current?.click()}
        >
          {uploading ? (
            <>
              <Loader2 size={36} className="spin" />
              <p>Subiendo imagen...</p>
            </>
          ) : (
            <>
              <div className="image-uploader__dropzone-icon">
                {dragActive ? <Image size={36} /> : <Upload size={36} />}
              </div>
              <p className="image-uploader__dropzone-text">
                {dragActive
                  ? 'Suelta la imagen aquí'
                  : 'Arrastra una imagen aquí o haz clic para seleccionar'}
              </p>
              <span className="image-uploader__dropzone-hint">
                JPG, PNG o WebP • Máximo 10MB
              </span>
            </>
          )}
        </div>
      )}

      {/* Error */}
      {error && (
        <div className="image-uploader__error">
          <AlertCircle size={14} />
          <span>{error}</span>
        </div>
      )}

      {/* URL actual (read-only info) */}
      {value && !uploading && (
        <div className="image-uploader__url">
          <span>URL:</span>
          <code>{value.length > 60 ? value.substring(0, 60) + '...' : value}</code>
        </div>
      )}

      {/* Hidden file input */}
      <input
        ref={inputRef}
        type="file"
        accept="image/jpeg,image/png,image/webp"
        onChange={handleInputChange}
        style={{ display: 'none' }}
      />
    </div>
  );
}
