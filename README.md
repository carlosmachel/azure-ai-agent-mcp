# Azure AI Agent MCP
## Visão Geral

Este projeto é uma aplicação web .NET que fornece uma API para criar e gerenciar agentes de IA Azure com integração MCP (Model Context Protocol).
Ele permite aos usuários criar agentes de IA, gerenciar threads de conversação e executar interações de IA com especificações de API através de um conjunto de endpoints HTTP.

## Funcionalidades
- Criação de agentes de IA personalizados com instruções específicas
- Criação de threads de conversação para interações com agentes
- Execução de tarefas do agente e recuperação de respostas geradas pela IA
- Integração com Azure AI Agents e ferramentas MCP para pesquisa de especificações de API
- Fluxo de trabalho automatizado de aprovação de ferramentas para interações MCP

## Pré-requisitos
- SDK .NET 9.0
- Assinatura Azure com serviços Azure AI
- Credenciais Azure apropriadas para autenticação

## Tecnologias Utilizadas
- ASP.NET Core 9.0
- Azure AI Agents Persistent SDK
- Azure AI Projects SDK
- Scalar.AspNetCore para documentação de API

## Estrutura do Projeto
- Ponto de entrada da aplicação e configuração de serviços **Program.cs**
- Definições de endpoints da API **Application/Module.cs**
- Implementação do serviço principal para interações com agentes de IA **Application/Service.cs**
- Configurações para serviços Azure AI **Application/AzureAiSettings.cs**

## Configuração
1. Clone o repositório
2. Configure suas definições do Azure AI no arquivo : `appsettings.json`

```json
   {
     "AzureAiSettings": {
       "Uri": "seu-endpoint-azure-ai",
       "Model": "seu-modelo-azure-ai"
     }
   }

```
3. Compile e execute a aplicação:

```bash
   dotnet build
   dotnet run
```

## Endpoints da API
### Criar Agente de IA

```
POST /ai-agent?name={nomeDoAgente}&instructions={instrucoesDoAgente}
```
Cria um novo agente de IA com o nome e instruções especificados.

### Criar Thread
```json
GET /ai-agent/create-thread
```
Cria um novo thread de conversação para interações com o agente.

### Executar Agente
```
GET /ai-agent/run?agentId={idDoAgente}&threadId={idDoThread}&userInput={entradaDoUsuario}
```
Executa um agente de IA com a entrada especificada e retorna a resposta gerada.

## Autenticação
A aplicação utiliza o DefaultAzureCredential para autenticação nos serviços Azure. Certifique-se de ter as funções e permissões Azure adequadas configuradas.

## Integração de Ferramentas MCP
A aplicação integra-se com ferramentas MCP (Model Context Protocol) para pesquisar e acessar especificações da API REST do Azure. A integração requer aprovação para uso de ferramentas, que é tratada automaticamente pelo serviço.

## Licença
Consulte o arquivo LICENSE para obter detalhes.



