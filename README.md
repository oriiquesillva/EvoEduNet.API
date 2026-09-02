# 🚀 EvoEduNet.API - Sistema de Controle de Matrículas Escolares

API RESTful desenvolvida em **.NET Framework 4.8** com **ASP.NET Web API 2**, **SQL Server** e **Dapper** (com consultas SQL escritas manualmente), seguindo arquitetura em camadas, princípios SOLID, transações atômicas ACID e tratamento semântico estrito de status HTTP.

---

## 📋 Índice
- [Visão Geral](#-visão-geral)
- [Tecnologias Utilizadas](#-tecnologias-utilizadas)
- [Arquitetura da Solução](#-arquitetura-da-solução)
- [Pré-requisitos](#-pré-requisitos)
- [Passo a Passo para Execução](#-passo-a-passo-para-execução)
  - [1. Configuração do Banco de Dados](#1-configuração-do-banco-de-dados)
  - [2. Configuração da Connection String](#2-configuração-da-connection-string)
  - [3. Compilação e Execução da API](#3-compilação-e-execução-da-api)
  - [4. Execução dos Testes Unitários](#4-execução-dos-testes-unitários)
  - [5. Interface Web (Frontend)](#5-interface-web-frontend)
- [Documentação dos Endpoints RESTful](#-documentação-dos-endpoints-restful)
- [Itens Bônus Implementados](#-itens-bônus-implementados)
  - [1. Cache e Transição para Redis](#1-estratégia-de-cache-e-como-seria-com-redis)
  - [2. Testes Unitários com Mocks](#2-testes-unitários-moq--nunit)
  - [3. Interface Web SPA](#3-interface-web-spa)
- [Status HTTP e Tratamento de Erros](#-status-http-e-tratamento-de-erros)

---

## 🎯 Visão Geral

O projeto foi construído para solucionar o desafio de controle de matrículas escolares com regras de negócio críticas e garantia de consistência de dados:
- **CRUD Completo de Alunos:** Listagem paginada com filtro por nome e total de registros, busca por ID, cadastro, atualização e **exclusão lógica** (*soft delete*).
- **Controle de Turmas:** Listagem com controle de vagas disponíveis em tempo real.
- **Matrículas com Transação Atômica ACID:** Operação que insere a matrícula e decrementa as vagas da turma dentro de uma mesma `IDbTransaction` do SQL Server.
- **Relatório Agregado em SQL Nativo:** Consulta agregada usando `LEFT JOIN` e `GROUP BY` no SQL Server sem processamento em memória.

---

## 🛠 Tecnologias Utilizadas

- **Linguagem & Framework:** C# 7.3 / .NET Framework 4.8
- **Web API & Hosting:** ASP.NET Web API 2 com **OWIN Self-Host** (`Microsoft.AspNet.WebApi.OwinSelfHost`)
- **Acesso a Dados:** **Dapper 2.1** com consultas SQL nativas (sem Entity Framework ou ORMs que gerem SQL)
- **Banco de Dados:** Microsoft SQL Server (compatível com Express, LocalDB ou Server)
- **Serialização:** Newtonsoft.Json (com CamelCase e formatação ISO-8601)
- **CORS:** Microsoft.Owin.Cors / Microsoft.AspNet.WebApi.Cors
- **Testes Unitários:** NUnit 4.1, Moq 4.20, Microsoft.NET.Test.Sdk
- **Frontend:** HTML5, Bootstrap 5.3, jQuery 3.7, FontAwesome 6

---

## 🏛 Arquitetura da Solução

O projeto segue estritamente a separação em camadas, garantindo que regras de negócio não fiquem acopladas às Controllers:

```text
EvoEduNet48.API/
├── EvoEduNet.API/                    # Projeto Principal da Web API (.NET 4.8)
│   ├── Controllers/                  # Controllers RESTful (Skinny Controllers)
│   │   ├── AlunosController.cs       # CRUD de alunos e paginação
│   │   ├── TurmasController.cs       # Consulta de turmas e vagas
│   │   ├── MatriculasController.cs   # Processamento de matrículas
│   │   ├── RelatoriosController.cs   # Relatório analítico agregado
│   │   └── StatusController.cs       # Health-check da API
│   ├── Domain/                       # Entidades, DTOs e Exceções de Domínio
│   │   ├── Entities/                 # Aluno, Turma, Matricula
│   │   ├── Dtos/                     # PagedResultDto, AlunoDtos, TurmaDtos, MatriculaDtos, RelatorioDtos
│   │   └── Exceptions/               # BusinessException (409), NotFoundException (404), ValidationException (400)
│   ├── Services/                     # Regras de Negócio e Transações ACID
│   │   ├── Interfaces/               # IAlunoService, ITurmaService, IMatriculaService, IRelatorioService, ITurmaCacheService
│   │   ├── AlunoService.cs
│   │   ├── TurmaService.cs
│   │   ├── MatriculaService.cs       # Transação atômica IDbTransaction
│   │   └── RelatorioService.cs
│   ├── Repositories/                 # Camada de Acesso a Dados (Dapper + SQL Puro)
│   │   ├── Interfaces/               # IAlunoRepository, ITurmaRepository, IMatriculaRepository, IRelatorioRepository
│   │   ├── AlunoRepository.cs        # Queries manuais, OFFSET/FETCH e SCOPE_IDENTITY()
│   │   ├── TurmaRepository.cs        # Decremento atômico com salvaguarda de concorrência
│   │   ├── MatriculaRepository.cs    # Inserção transacionada e verificação de duplicidade
│   │   └── RelatorioRepository.cs    # LEFT JOIN + GROUP BY nativo
│   ├── Infrastructure/               # Suporte de Infraestrutura
│   │   ├── Data/                     # IDbConnectionFactory e SqlConnectionFactory
│   │   ├── Filters/                  # ValidateModelAttribute (400) e CustomExceptionFilterAttribute (404, 409, 500)
│   │   ├── IoC/                      # SimpleDependencyResolver (Injeção de dependência nativa Web API 2)
│   │   └── Cache/                    # MemoryTurmaCacheService (Padrão Cache-Aside)
│   ├── Startup.cs                    # Configuração de rotas, middlewares, CORS, JSON e DI
│   ├── Program.cs                    # Host de console OWIN
│   └── App.config                    # ConnectionString e configurações de host
├── EvoEduNet.Tests/                  # Projeto de Testes Unitários (NUnit + Moq)
│   ├── MatriculaServiceTests.cs      # 8 testes cobrindo transações, rollback e regras de matrícula
│   ├── TurmaServiceTests.cs          # Testes de cache HIT/MISS
│   └── AlunoServiceTests.cs          # Testes de validação de duplicidade e soft delete
├── frontend/                         # Interface Web SPA
│   └── index.html                    # Interface HTML5/Bootstrap/jQuery para consumo da API
├── script-banco.sql                  # Script DDL e DML de criação da base de dados
└── EvoEduNet.API.slnx                # Arquivo da Solução
```

---

## 📋 Pré-requisitos

1. **Sistema Operacional:** Windows 10/11 ou Windows Server.
2. **.NET Framework:** .NET Framework 4.8 Runtime e Developer Pack instalados.
3. **Compilador / IDE:** Visual Studio 2019/2022/Visual Studio Community com suporte a desenvolvimento .NET Desktop / Web, ou **MSBuild 15+**.
4. **Banco de Dados:** Microsoft SQL Server (qualquer edição: Express, LocalDB ou Server padrão).

---

## 🚀 Passo a Passo para Execução

### 1. Configuração do Banco de Dados
Abra o SQL Server Management Studio (SSMS), Azure Data Studio ou sqlcmd e execute o script [script-banco.sql](script-banco.sql) localizado na raiz do repositório:
- O script criará a base `TesteEscola`, as tabelas `Aluno`, `Turma`, `Matricula` e inserirá a carga de dados inicial.

### 2. Configuração da Connection String
Verifique o arquivo `EvoEduNet.API/App.config`. A connection string padrão configurada é:

```xml
<connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Server=localhost;Database=TesteEscola;Integrated Security=True;TrustServerCertificate=True;" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

> **Nota:** Caso utilize SQL Server Express ou LocalDB, basta ajustar o parâmetro `Server`:
> - Para SQL Express: `Server=localhost\SQLEXPRESS;Database=TesteEscola;Integrated Security=True;TrustServerCertificate=True;`
> - Para LocalDB: `Server=(localdb)\MSSQLLocalDB;Database=TesteEscola;Integrated Security=True;TrustServerCertificate=True;`

### 3. Compilação e Execução da API
Você pode compilar e executar o projeto diretamente pelo Visual Studio pressionando **F5**, ou via linha de comando:

```powershell
# Compilar com MSBuild
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" EvoEduNet.API\EvoEduNet.API.csproj

# Executar a API
.\EvoEduNet.API\bin\Debug\EvoEduNet.API.exe
```

O servidor iniciará automaticamente em: **`http://localhost:5000/`**

### 4. Execução dos Testes Unitários
Para rodar a suíte completa de testes unitários:

```powershell
# Execução via vstest.console.exe (ou pelo Gerenciador de Testes do Visual Studio)
& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" EvoEduNet.Tests\bin\Debug\EvoEduNet.Tests.dll
```
**Resultado Esperado:** 14 testes executados com 100% de aprovação.

### 5. Interface Web (Frontend)
Para utilizar a interface gráfica:
1. Com a API em execução (`http://localhost:5000/`), abra o arquivo [frontend/index.html](frontend/index.html) em qualquer navegador moderno.
2. A tela carregará os alunos paginados, com opções de busca por nome, troca de páginas, cadastro e inativação de alunos em tempo real.

---

## 📡 Documentação dos Endpoints RESTful

### 1. Health-Check
- **`GET /api/status`**
  - **Retorno (200 OK):**
    ```json
    {
      "status": "Online",
      "projeto": "EvoEduNet.API",
      "versao": "1.0.0",
      "plataforma": ".NET Framework 4.8",
      "dataHora": "2026-09-02T16:25:30"
    }
    ```

---

### 2. Módulo Alunos (CRUD Completo)
- **`GET /api/alunos?page=1&pageSize=5&nome=`**
  - Retorna lista paginada com metadados e total de registros.
  - **Retorno (200 OK):**
    ```json
    {
      "total": 8,
      "page": 1,
      "pageSize": 5,
      "totalPages": 2,
      "items": [
        {
          "id": 1,
          "nome": "Ana Souza",
          "email": "ana.souza@email.com",
          "dataNascimento": "2006-03-14T00:00:00",
          "ativo": true,
          "dataCadastro": "2026-09-02T14:40:00"
        }
      ]
    }
    ```

- **`GET /api/alunos/{id}`**
  - Busca aluno por identificador único.
  - **Retorno:** `200 OK` ou `404 Not Found`.

- **`POST /api/alunos`**
  - Cadastra um novo aluno.
  - **Payload:**
    ```json
    {
      "nome": "Lucas Ferreira",
      "email": "lucas.ferreira@email.com",
      "dataNascimento": "2005-04-10"
    }
    ```
  - **Retorno:** `201 Created` ou `400 Bad Request` (validação) / `409 Conflict` (e-mail duplicado).

- **`PUT /api/alunos/{id}`**
  - Atualiza os dados de um aluno.
  - **Retorno:** `200 OK` ou `404 Not Found` / `409 Conflict`.

- **`DELETE /api/alunos/{id}`**
  - **Exclusão Lógica:** Altera o campo `Ativo` para `0` no banco de dados sem apagar o registro físico.
  - **Retorno (200 OK):**
    ```json
    {
      "mensagem": "Aluno com ID 1 inativado com sucesso."
    }
    ```

---

### 3. Módulo Turmas
- **`GET /api/turmas`**
  - Lista as turmas escolares exibindo as vagas restantes (`VagasDisponiveis`). Possui cache inteligente com invalidação após novas matrículas.
  - **Retorno (200 OK):**
    ```json
    [
      {
        "id": 1,
        "nome": "3A - Ensino Medio",
        "periodo": "Manha",
        "vagasTotal": 30,
        "vagasDisponiveis": 28
      }
    ]
    ```

---

### 4. Módulo Matrículas (Transação Atômica ACID)
- **`POST /api/matriculas`**
  - Realiza a matrícula do aluno na turma.
  - **Payload:**
    ```json
    {
      "alunoId": 1,
      "turmaId": 2
    }
    ```
  - **Regras de Negócio Validadas:**
    1. O aluno deve existir (`404 Not Found`).
    2. O aluno deve estar ativo (`409 Conflict`).
    3. A turma deve existir (`404 Not Found`).
    4. A turma deve possuir vagas disponíveis > 0 (`409 Conflict`).
    5. O aluno não pode estar matriculado duas vezes na mesma turma (`409 Conflict`).
    6. **Atomicidade:** A inserção em `Matricula` e o decremento em `Turma` ocorrem dentro de uma `IDbTransaction`. Se qualquer etapa falhar, ocorre `Rollback`.
  - **Retorno de Sucesso (201 Created):**
    ```json
    {
      "id": 9,
      "alunoId": 1,
      "nomeAluno": "Ana Souza",
      "turmaId": 2,
      "nomeTurma": "3B - Ensino Medio",
      "dataMatricula": "2026-09-02T16:50:00",
      "mensagem": "Matrícula realizada com sucesso para o aluno 'Ana Souza' na turma '3B - Ensino Medio'."
    }
    ```

---

### 5. Módulo Relatórios
- **`GET /api/relatorios/alunos-por-turma`**
  - Retorna agregação analítica nativa feita via SQL (`LEFT JOIN` e `GROUP BY` no SQL Server).
  - **Retorno (200 OK):**
    ```json
    [
      {
        "nomeTurma": "3A - Ensino Medio",
        "periodo": "Manha",
        "totalAlunosMatriculados": 2,
        "vagasRestantes": 28
      },
      {
        "nomeTurma": "3B - Ensino Medio",
        "periodo": "Tarde",
        "totalAlunosMatriculados": 0,
        "vagasRestantes": 30
      }
    ]
    ```

---

## 🎁 Itens Bônus Implementados

### 1. Estratégia de Cache e Como Seria com Redis
- Criamos a interface `ITurmaCacheService` e a implementação thread-safe `MemoryTurmaCacheService` com TTL de 5 minutos.
- O endpoint `GET /api/turmas` adota o padrão **Cache-Aside** (consulta o cache; se vazio, busca no banco e preenche o cache).
- No `MatriculaService`, imediatamente após o `Commit` da transação, o cache é invalidado chamando `await _turmaCacheService.InvalidarAsync()`.
- **Como seria com Redis em Produção?**
  Basta instalar o pacote `StackExchange.Redis`, criar a classe `RedisTurmaCacheService` implementando a mesma interface `ITurmaCacheService` e registrar no `Startup.cs`:
  ```csharp
  // Exemplo de implementação com Redis real:
  public class RedisTurmaCacheService : ITurmaCacheService
  {
      private readonly IDatabase _db;
      public RedisTurmaCacheService(string cnn) => _db = ConnectionMultiplexer.Connect(cnn).GetDatabase();
      public async Task<IEnumerable<TurmaResponseDto>> ObterTurmasAsync() {
          var data = await _db.StringGetAsync("turmas:listagem");
          return data.IsNullOrEmpty ? null : JsonConvert.DeserializeObject<IEnumerable<TurmaResponseDto>>(data);
      }
      public async Task InserirAsync(IEnumerable<TurmaResponseDto> t) =>
          await _db.StringSetAsync("turmas:listagem", JsonConvert.SerializeObject(t), TimeSpan.FromMinutes(5));
      public async Task InvalidarAsync() => await _db.KeyDeleteAsync("turmas:listagem");
  }
  ```

### 2. Testes Unitários (Moq + NUnit)
- Projeto `EvoEduNet.Tests` com **14 testes unitários automatizados**.
- Cobertura completa de regras de negócio, testes de concorrência simulada, validação de chamadas a `Commit()` e `Rollback()` e invalidação de cache.

### 3. Interface Web SPA
- Single-Page Application desenvolvida em [frontend/index.html](frontend/index.html) permitindo consumir a listagem com paginação dinâmica, filtros instantâneos, modal para cadastro e inativação de alunos com atualização em tempo real.

---

## 🛡 Status HTTP e Tratamento de Erros

A API adota mapeamento estrito via filtros globais (`ValidateModelAttribute` e `CustomExceptionFilterAttribute`), garantindo que **nunca seja retornado status 500 para regras de negócio ou validação**:

| Status | Significado | Exemplo de Aplicação |
|---|---|---|
| **`200 OK`** | Operação realizada com sucesso | Consultas GET, atualizações PUT e exclusão lógica DELETE. |
| **`201 Created`** | Recurso criado com sucesso | Cadastro de aluno (`POST /api/alunos`) e matrícula (`POST /api/matriculas`). |
| **`400 Bad Request`** | Erro de validação na requisição | Campos obrigatórios ausentes, e-mail mal formatado, DTO nulo. |
| **`404 Not Found`** | Recurso não encontrado | ID de aluno ou turma inexistente. |
| **`409 Conflict`** | Violação de regra de negócio | Turma sem vagas, aluno inativo, matrícula duplicada, e-mail já em uso. |
| **`500 Internal Server Error`** | Erro interno inesperado | Falhas não tratadas de infraestrutura (com mensagem mascarada por segurança). |

---

## 👨‍💻 Autor
Desenvolvido como solução técnica para o Teste Prático .NET Pleno.
