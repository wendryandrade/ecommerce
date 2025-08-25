#!/bin/bash
set -e

DB_HOST=${DB_HOST:-ecommerce-db}
DB_PORT=${DB_PORT:-1433}
TIMEOUT=${TIMEOUT:-60}

# Funcao para esperar por uma porta TCP
wait_for_port() {
  local host="$1"
  local port="$2"
  local timeout="$3"
  local start_time=$(date +%s)

  echo "Aguardando o servico $host:$port..."

  while ! timeout 1 bash -c "echo > /dev/tcp/$host/$port"; do
    local elapsed_time=$(($(date +%s) - $start_time))
    if [ "$elapsed_time" -ge "$timeout" ]; then
      echo "Tempo esgotado ($timeout segundos) esperando por $host:$port"
      exit 1
    fi
    sleep 1
  done

  echo "Servico $host:$port esta pronto!"
}

# Chama a funcao de espera para o SQL Server
wait_for_port "$DB_HOST" "$DB_PORT" "$TIMEOUT"

CERT_PATH="/https/aspnetapp.pfx"
CERT_PASSWORD="Pass@word1"

if [ -f "$CERT_PATH" ]; then
  export ASPNETCORE_URLS="http://0.0.0.0:80;https://0.0.0.0:443"
  export ASPNETCORE_Kestrel__Certificates__Default__Path="$CERT_PATH"
  export ASPNETCORE_Kestrel__Certificates__Default__Password="$CERT_PASSWORD"
  echo "Using mounted dev cert - running HTTPS"
else
  export ASPNETCORE_URLS="http://0.0.0.0:80"
  echo "No dev cert mounted - running HTTP only"
fi

# Inicia a API (.NET 8 aplica migrations automaticamente)
echo "Iniciando a API..."
exec dotnet Ecommerce.API.dll