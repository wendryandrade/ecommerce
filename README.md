# Ecommerce - Setup Local com Docker

Este guia explica como rodar o projeto **Ecommerce** localmente usando Docker, incluindo API, banco de dados, RabbitMQ e HTTPS opcional.

---

## Pré-requisitos

* [Docker Desktop](https://www.docker.com/products/docker-desktop) (Windows/Mac) ou Docker Engine (Linux)
* Git
* .NET 8 SDK (para gerar certificados HTTPS, se desejar)

---

## Passo a passo para rodar o sistema

### 1. Instalar Docker

Baixe e instale o Docker Desktop (Windows/Mac) ou Docker Engine (Linux).

---

### 2. Clonar o repositório

```bash
git clone https://github.com/wendryandrade/ecommerce.git
cd ecommerce
```

---

### 3. Configurar variáveis de ambiente

Antes de buildar os containers, copie o arquivo de exemplo `.env.example` para `.env`:

```bash
cp .env.example .env
```

Edite o `.env` para ajustar as integrações externas:

* **Stripe**: altere `STRIPE_SECRETKEY` e `STRIPE_PUBLISHABLEKEY` com suas chaves da conta Stripe.
* **Melhor Envio**: altere `API_TOKEN` com seu token de acesso da conta Melhor Envio.

> ⚠️ É importante manter o `.env` atualizado com suas chaves para que as integrações funcionem corretamente.

---

### 4. Rodar os containers

Na raiz do projeto, execute:

```bash
docker compose up --build -d
```

Isso iniciará:

* **API** (`Ecommerce.API`)
* **SQL Server**
* **RabbitMQ**

---

### 5. Configurar HTTPS (Opcional)

Para habilitar HTTPS no container da API:

1. Crie a pasta `./certs` na raiz do projeto.
2. Gere um certificado de desenvolvimento:

```bash
dotnet dev-certs https -ep ./certs/aspnetapp.pfx -p Pass@word1
```

3. Confie no certificado para remover aviso de "não seguro":

```bash
dotnet dev-certs https --trust
```

4. Recrie os containers para aplicar o certificado:

```bash
docker compose up --build -d
```

**Notas importantes:**

* A senha do certificado deve ser a mesma configurada no `entrypoint.sh` (`Pass@word1`).
* Em Linux/Mac, use o caminho equivalente (ex.: `~/.aspnet/https`) e adicione manualmente o certificado ao sistema (`update-ca-certificates`).
* Sempre reconstrua a imagem após gerar o certificado.

---

### 6. Acessos locais

* **API (Swagger):** [https://localhost:8081/swagger/index.html](https://localhost:8081/swagger/index.html)
* **RabbitMQ:** [http://localhost:15672](http://localhost:15672)

---

### 7. Parar e remover containers

Para parar os containers e limpar volumes:

```bash
docker compose down -v
```

---

### Informações adicionais

- Ajuste de line endings do entrypoint
  - Se ocorrer erro "/entrypoint.sh: not found" ao subir os containers no Linux/WSL, converta o arquivo Ecommerce.API/entrypoint.sh para final de linha LF (Unix):
    - VS Code: abra o arquivo, no canto inferior direito selecione CRLF e altere para LF, salve.
    - Ou via terminal (em ambientes que possuam dos2unix): dos2unix Ecommerce.API/entrypoint.sh
      
- .env.example
  - O arquivo .env.example contém variáveis de ambiente para facilitar a configuração local e do docker-compose.
  - Foi incluída uma chave JWT válida para testes: ajuste conforme necessário antes de usar em produção.


### Observações

* Todos os serviços rodam em containers separados, mas podem se comunicar entre si usando os nomes dos serviços definidos no `docker-compose.yml`.
* Se ocorrerem problemas de porta ocupada, ajuste os mapeamentos no `docker-compose.yml`.
* A API aplica migrations automaticamente ao iniciar.
* Sempre atualize o `.env` ao alterar chaves de integração ou outras configurações.

---

### Contato

Para dúvidas ou problemas, abra uma issue no repositório ou entre em contato com o desenvolvedor.
