#!/bin/bash
set -e

DB_HOST=${DB_HOST:-ecommerce-db}
DB_PORT=${DB_PORT:-1433}
TIMEOUT=${TIMEOUT:-60}

# Função para esperar por uma porta TCP
wait_for_port() {
  local host="$1"
  local port="$2"
  local timeout="$3"
  local start_time=$(date +%s)

  echo "Aguardando o serviço $host:$port..."

  while ! timeout 1 bash -c "echo > /dev/tcp/$host/$port"; do
    local elapsed_time=$(($(date +%s) - $start_time))
    if [ "$elapsed_time" -ge "$timeout" ]; then
      echo "Tempo esgotado ($timeout segundos) esperando por $host:$port"
      exit 1
    fi
    sleep 1
  done

  echo "Serviço $host:$port está pronto!"
}

# Chama a função de espera para o SQL Server
wait_for_port "$DB_HOST" "$DB_PORT" "$TIMEOUT"

# Inicia a API (.NET 8 aplica migrations automaticamente)
echo "Iniciando a API..."
exec dotnet Ecommerce.API.dll