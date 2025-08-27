# Configuração de Email - E-commerce

## Problema Identificado

O sistema de envio de email estava implementado apenas como um **mock** que apenas fazia log das mensagens, não enviando emails reais.

## Solução Implementada

Foi implementado um serviço de email real usando **SMTP** com as seguintes funcionalidades:

### 1. Configuração de Email

Adicione as seguintes variáveis de ambiente ao seu arquivo `.env`:

```bash
# Email Configuration
EMAIL_SMTP_SERVER=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_SMTP_USERNAME=your-email@gmail.com
EMAIL_SMTP_PASSWORD=your-app-password-here
EMAIL_FROM_EMAIL=noreply@ecommerce.com
EMAIL_FROM_NAME=E-commerce Store
EMAIL_ENABLE_SSL=true
```

### 2. Configuração para Gmail

Para usar Gmail como servidor SMTP:

1. **Ative a verificação em duas etapas** na sua conta Google
2. **Gere uma senha de app**:
   - Vá para https://myaccount.google.com/apppasswords
   - Selecione "Email" como aplicativo
   - Use a senha gerada no `EMAIL_SMTP_PASSWORD`

### 3. Configuração para Outlook/Hotmail

```bash
EMAIL_SMTP_SERVER=smtp-mail.outlook.com
EMAIL_SMTP_PORT=587
EMAIL_SMTP_USERNAME=your-email@outlook.com
EMAIL_SMTP_PASSWORD=your-password
EMAIL_ENABLE_SSL=true
```

### 4. Configuração para outros provedores

- **Yahoo**: `smtp.mail.yahoo.com:587`
- **Provedor local**: Consulte seu provedor de email

## Como Funciona

1. **OrderConsumer** processa o pedido e publica `OrderProcessedEvent`
2. **EmailConsumer** recebe o evento via RabbitMQ
3. **EmailService** envia o email real usando SMTP
4. Se as configurações não estiverem definidas, apenas faz log (comportamento de fallback)

## Testando o Envio

1. Configure as variáveis de ambiente
2. Faça uma compra no sistema
3. Verifique os logs para confirmar o envio
4. Verifique sua caixa de entrada

## Próximos Passos

- [x] Buscar email real do usuário no banco de dados
- [x] Implementar templates de email mais elaborados
- [ ] Adicionar suporte a outros provedores (SendGrid, Mailgun, etc.)
- [ ] Implementar retry em caso de falha
- [ ] Adicionar fila de emails para processamento assíncrono
- [ ] Implementar notificações de status do pedido
- [ ] Adicionar rastreamento de entrega

## Troubleshooting

### Email não chega
1. Verifique se as configurações SMTP estão corretas
2. Confirme se a senha de app está correta (Gmail)
3. Verifique se o firewall não está bloqueando a porta SMTP
4. Consulte os logs da aplicação para erros

### Erro de autenticação
- Gmail: Use senha de app, não a senha da conta
- Outlook: Pode precisar ativar "Acesso a app menos seguro"

### Erro de SSL
- Certifique-se de que `EMAIL_ENABLE_SSL=true`
- Para Gmail, use porta 587 com SSL ou 465 com SSL
