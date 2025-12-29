#!/bin/bash
# Script de despliegue a Google Cloud Run
# Requiere: gcloud CLI instalado y configurado

echo "=== Desplegando a Google Cloud Run ==="

# Obtener el PROJECT_ID
PROJECT_ID=$(gcloud config get-value project)
if [ -z "$PROJECT_ID" ]; then
    echo "Error: No se encontró PROJECT_ID. Configura el proyecto con: gcloud config set project PROJECT_ID"
    exit 1
fi

echo "Proyecto: $PROJECT_ID"

# Construir la imagen Docker
echo ""
echo "=== Construyendo imagen Docker ==="
docker build -t gcr.io/$PROJECT_ID/calificacion-puntos-web .

if [ $? -ne 0 ]; then
    echo "Error al construir la imagen Docker"
    exit 1
fi

# Subir la imagen a Google Container Registry
echo ""
echo "=== Subiendo imagen a GCR ==="
docker push gcr.io/$PROJECT_ID/calificacion-puntos-web

if [ $? -ne 0 ]; then
    echo "Error al subir la imagen"
    exit 1
fi

# Desplegar a Cloud Run
echo ""
echo "=== Desplegando a Cloud Run ==="
gcloud run deploy calificacion-puntos-web \
    --image gcr.io/$PROJECT_ID/calificacion-puntos-web \
    --region us-central1 \
    --platform managed \
    --allow-unauthenticated

if [ $? -ne 0 ]; then
    echo "Error al desplegar"
    exit 1
fi

echo ""
echo "=== Despliegue completado exitosamente ==="
echo "Obtén la URL con: gcloud run services describe calificacion-puntos-web --region us-central1 --format 'value(status.url)'"

