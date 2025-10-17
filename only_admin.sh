TAG=dev
APP=admin
echo "Changed into $PWD"
git pull
git submodule update --init --recursive

GIT_COMMIT=$(git rev-parse HEAD) 

echo $GIT_COMMIT


echo GIT_COMMIT=$(git rev-parse HEAD) | cat > .env

echo "start build"





docker compose build dicom-admin-server --build-arg GIT_COMMIT=${GIT_COMMIT}
#docker compose build dicom-admin-client --build-arg GIT_COMMIT=${GIT_COMMIT}

echo "end build"

#docker compose build web-server --build-arg GIT_COMMIT=${GIT_COMMIT}

echo "Pushing images to ghcr.io registry..."
#docker push ghcr.io/lotfiaghel/dicom-admin-server:latest
#docker push ghcr.io/lotfiaghel/dicom-admin-client:latest
#docker push ghcr.io/lotfiaghel/dicom-web-server:latest

echo "Image pushed, you can now update deployment  to $APP:$TAG"

