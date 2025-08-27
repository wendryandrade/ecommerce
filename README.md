# 🛍️ Ecommerce

Este projeto é uma **API de ecommerce** desenvolvida em **.NET 8** e banco de dados **SQL Server**, utilizando **Entity Framework Core (EF Core)** como ORM.

A aplicação segue boas práticas utilizando o padrão **Clean Architecture**, utiliza **CQRS** (Command Query Responsibility Segregation), testes unitários e de integração utilizando **XUnit**, processamento assíncrono com mensageria utilizando **RabbitMQ**, integrações com serviços externos de pagamento **Stripe**, frete **MelhorEnvio** e CEP **ViaCEP**.

Toda a aplicação pode ser executada localmente de forma simples utilizando **Docker Compose**.

---

## 🚀 Funcionalidades

A API oferece os principais recursos de um ecommerce:

- 🔑 **Autenticação & Autorização**  
  - Registro de usuários  
  - Login com **JWT**  
  - Controle de acesso por roles (**Admin** / **Customer**)  

- 📦 **Gerenciamento de Produtos e Categorias**  
  - Listagem, criação, atualização e exclusão  
  - Acesso restrito a administradores  

- 🛒 **Carrinho de Compras**  
  - Adicionar, remover e gerenciar quantidade de itens  

- 💳 **Checkout**  
  - Finalização de compra com dados de entrega, CEP e pagamento  

- ⚡ **Pedidos Assíncronos**  
  - Checkout publica evento em fila 
  - Não bloqueia o fluxo  

- 🔗 **Integrações Externas**  
  - **Frete:** cálculo de envio e informações de CEP 
  - **Pagamento:** processamento de transações 

---

## 🛠️ Tecnologias Utilizadas

- .NET 8  
- SQL Server  
- Entity Framework Core (EF Core)  
- RabbitMQ  
- Stripe, MelhorEnvio, ViaCEP  
- XUnit   
- SonarQube Cloud
- Docker e Docker Compose

---

## ⚙️ Como Executar o Projeto

### 🔧 Pré-requisitos
- [Docker Desktop](https://www.docker.com/products/docker-desktop) ou Docker Engine (Linux)  
- [Git](https://git-scm.com/)  

### 📂 1. Clonar o Repositório
```bash
git clone https://github.com/wendryandrade/ecommerce.git
cd ecommerce
```

### 🔑 2. Configurar Variáveis de Ambiente
Copie o arquivo de exemplo `.env.example` para `.env`:
```bash
cp .env.example .env
```

Edite com suas credenciais:  
- `DB_PASS=Your_password123!`  
- `JWT_KEY=...`  
- `STRIPE_SECRETKEY=...`  
- `API_TOKEN=...`  

⚠️ **Atenção:** o `.env` deve estar atualizado para que as integrações funcionem corretamente.

### 🐳 3. Subir os Serviços com Docker
```bash
docker compose up --build -d
```
Isso irá iniciar:
- API .NET  
- SQL Server  
- RabbitMQ  
- Aplicar migrations e seed automaticamente com **EF Core**

### 🔐 4. Configurar HTTPS
Caso precise consumir APIs externas com HTTPS (Opcional):  
1. Criar pasta `certs` na raiz  
2. Gerar certificado:
   ```bash
   dotnet dev-certs https -ep ./certs/aspnetapp.pfx -p Pass@word1
   dotnet dev-certs https --trust
   ```
3. Recriar containers:
   ```bash
   docker compose up --build -d
   ```

**Notas importantes:**

- A senha do certificado deve ser a mesma configurada no entrypoint.sh (Pass@word1).
- Em Linux/Mac, use o caminho equivalente (ex.: ~/.aspnet/https) e adicione manualmente o certificado ao sistema (update-ca-certificates).
- Sempre recrie o container após gerar o certificado.
---

## 🌐 Acessos e Interfaces

- **Swagger (API Docs):**  
  - `http://localhost:8080/swagger/index.html`  
  - `https://localhost:8081/swagger/index.html` (com HTTPS)  

- **RabbitMQ:**  
  - `http://localhost:15672`  
  - Usuário: `guest` / Senha: `guest`  

- **SQL Server:**  
  - `localhost:1433`  
  - Credenciais conforme `.env`  

🔑 Usuário Admin gerado automaticamente (via **DatabaseSeeder**):  
- E-mail: `admin@ecommerce.com`  
- Senha: `admin123`  

---

## 📊 Testes e Qualidade de Código

- Testes **unitários e de integração** com **XUnit**  
- Executados automaticamente no Docker  
- **CI/CD com GitHub Actions** → build, testes e análise no **SonarCloud**  

 
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=wendryandrade_ecommerce&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=wendryandrade_ecommerce)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=wendryandrade_ecommerce&metric=coverage)](https://sonarcloud.io/summary/new_code?id=wendryandrade_ecommerce)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=wendryandrade_ecommerce&metric=bugs)](https://sonarcloud.io/summary/new_code?id=wendryandrade_ecommerce)

> 🔗 [Ver análise completa no SonarCloud](https://sonarcloud.io/project/overview?id=wendryandrade_ecommerce)

---

## 🛑 Parar e Remover Containers

Para parar todos os containers e remover os volumes associados, execute:

```bash
docker compose down -v
```
