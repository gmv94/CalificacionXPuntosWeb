# Guía de Despliegue a Google Cloud Run

## Requisitos Previos

1. **Google Cloud SDK (gcloud CLI)** instalado
2. **Docker** instalado y funcionando
3. **Cuenta de Google Cloud** con proyecto configurado
4. **Permisos** para crear servicios en Cloud Run

## Pasos para Desplegar

### Opción 1: Usar el Script de Despliegue (Recomendado)

#### En Windows (PowerShell):
```powershell
.\deploy.ps1
```

#### En Linux/Mac:
```bash
chmod +x deploy.sh
./deploy.sh
```

### Opción 2: Despliegue Manual

#### 1. Configurar el proyecto de Google Cloud
```bash
gcloud config set project TU_PROJECT_ID
```

#### 2. Habilitar las APIs necesarias
```bash
gcloud services enable cloudbuild.googleapis.com
gcloud services enable run.googleapis.com
gcloud services enable containerregistry.googleapis.com
```

#### 3. Construir la imagen Docker
```bash
docker build -t gcr.io/TU_PROJECT_ID/calificacion-puntos-web .
```

#### 4. Subir la imagen a Google Container Registry
```bash
docker push gcr.io/TU_PROJECT_ID/calificacion-puntos-web
```

#### 5. Desplegar a Cloud Run
```bash
gcloud run deploy calificacion-puntos-web \
    --image gcr.io/TU_PROJECT_ID/calificacion-puntos-web \
    --region us-central1 \
    --platform managed \
    --allow-unauthenticated
```

### Opción 3: Usar Cloud Build (CI/CD)

Si tienes Cloud Build configurado, puedes usar el archivo `cloudbuild.yaml`:

```bash
gcloud builds submit --config cloudbuild.yaml
```

## Verificar el Despliegue

Después del despliegue, obtén la URL del servicio:

```bash
gcloud run services describe calificacion-puntos-web \
    --region us-central1 \
    --format 'value(status.url)'
```

## Actualizar un Despliegue Existente

Para actualizar la aplicación con nuevos cambios:

1. Realiza los cambios en el código
2. Ejecuta el script de despliegue nuevamente:
   ```bash
   ./deploy.sh
   ```
   o
   ```powershell
   .\deploy.ps1
   ```

## Solución de Problemas

### Error: "Permission denied"
- Verifica que tengas los permisos necesarios en Google Cloud
- Ejecuta: `gcloud auth login`

### Error: "Docker build failed"
- Verifica que Docker esté corriendo
- Revisa que el Dockerfile esté correcto

### Error: "Image push failed"
- Verifica que tengas permisos en Container Registry
- Ejecuta: `gcloud auth configure-docker`

### La aplicación no carga
- Verifica los logs: `gcloud run services logs read calificacion-puntos-web --region us-central1`
- Asegúrate de que el puerto 8080 esté configurado correctamente

## Variables de Entorno (Opcional)

Si necesitas configurar variables de entorno:

```bash
gcloud run services update calificacion-puntos-web \
    --region us-central1 \
    --set-env-vars "ASPNETCORE_ENVIRONMENT=Production"
```

## Notas

- El despliegue puede tardar varios minutos
- La primera vez que despliegas, Cloud Run creará el servicio
- Los despliegues posteriores actualizarán el servicio existente
- Los datos en memoria se perderán al reiniciar el servicio

