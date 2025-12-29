# Script de despliegue a Google Cloud Run
# Requiere: gcloud CLI instalado y configurado

Write-Host "=== Desplegando a Google Cloud Run ===" -ForegroundColor Green

# Obtener el PROJECT_ID
$PROJECT_ID = gcloud config get-value project
if (-not $PROJECT_ID) {
    Write-Host "Error: No se encontró PROJECT_ID. Configura el proyecto con: gcloud config set project PROJECT_ID" -ForegroundColor Red
    exit 1
}

Write-Host "Proyecto: $PROJECT_ID" -ForegroundColor Yellow

# Construir la imagen Docker
Write-Host "`n=== Construyendo imagen Docker ===" -ForegroundColor Green
docker build -t gcr.io/$PROJECT_ID/calificacion-puntos-web .

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al construir la imagen Docker" -ForegroundColor Red
    exit 1
}

# Subir la imagen a Google Container Registry
Write-Host "`n=== Subiendo imagen a GCR ===" -ForegroundColor Green
docker push gcr.io/$PROJECT_ID/calificacion-puntos-web

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al subir la imagen" -ForegroundColor Red
    exit 1
}

# Desplegar a Cloud Run
Write-Host "`n=== Desplegando a Cloud Run ===" -ForegroundColor Green
gcloud run deploy calificacion-puntos-web `
    --image gcr.io/$PROJECT_ID/calificacion-puntos-web `
    --region us-central1 `
    --platform managed `
    --allow-unauthenticated

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al desplegar" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== Despliegue completado exitosamente ===" -ForegroundColor Green
Write-Host "Obtén la URL con: gcloud run services describe calificacion-puntos-web --region us-central1 --format 'value(status.url)'" -ForegroundColor Yellow

