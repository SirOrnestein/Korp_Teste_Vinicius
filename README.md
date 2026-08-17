# Korp - Sistema de Estoque e Faturamento

Projeto desenvolvido como teste técnico para estágio em desenvolvimento.

A aplicação simula um sistema integrado de estoque e faturamento, permitindo cadastrar produtos, criar notas fiscais com múltiplos itens e realizar a baixa automática do estoque no fechamento da nota.

## Tecnologias utilizadas

### Frontend
- Angular
- TypeScript
- HTML
- CSS
- RxJS

### Backend
- C#
- ASP.NET Core
- Entity Framework Core
- LINQ

### Banco de dados
- PostgreSQL

## Arquitetura

O projeto foi dividido em três aplicações:

- `frontend`: interface desenvolvida em Angular.
- `estoque-api`: microsserviço responsável pelo cadastro, consulta e atualização do estoque.
- `faturamento-api`: microsserviço responsável pela criação e fechamento das notas fiscais.

Durante o fechamento de uma nota, o microsserviço de Faturamento se comunica com o microsserviço de Estoque para realizar a baixa dos produtos.

```text
Angular
   |
   |----> Estoque API ----> PostgreSQL (korp_estoque)
   |
   |----> Faturamento API ----> PostgreSQL (korp_faturamento)
                    |
                    |----> Estoque API
```

## Funcionalidades

- Cadastro de produtos com código, descrição e saldo.
- Listagem dos produtos cadastrados.
- Criação de notas fiscais.
- Inclusão de múltiplos produtos na mesma nota.
- Numeração sequencial das notas.
- Status de nota `Aberta` e `Fechada`.
- Confirmação antes do fechamento da nota.
- Indicador visual durante o processamento.
- Baixa automática do estoque.
- Validação de estoque insuficiente.
- Tratamento de indisponibilidade entre os microsserviços.
- Bloqueio da criação de notas sem cliente ou sem produtos.
- Mensagens visuais de sucesso e erro.
- Persistência dos dados em PostgreSQL.

## Configuração do banco de dados

O projeto utiliza dois bancos PostgreSQL:

```text
korp_estoque
korp_faturamento
```

Cada API possui um arquivo:

```text
appsettings.Example.json
```

Crie um `appsettings.json` em cada API com base no arquivo de exemplo e configure seu usuário e senha do PostgreSQL.

Exemplo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=NOME_DO_BANCO;Username=postgres;Password=SUA_SENHA"
  }
}
```

Os arquivos `appsettings.json` reais não são enviados ao repositório para evitar a exposição de credenciais.

## Executando o projeto

### Estoque API

```bash
cd estoque-api
dotnet run
```

### Faturamento API

```bash
cd faturamento-api
dotnet run
```

### Frontend

```bash
cd frontend
npm install
ng serve
```

A aplicação Angular estará disponível normalmente em:

```text
http://localhost:4200
```
## Endereços da aplicação

| Serviço | Endereço |
|---|---|
| Frontend Angular | `http://localhost:4200` |
| Estoque API | `http://localhost:5255` |
| Faturamento API | `http://localhost:5026` |

## Principais endpoints

#### Estoque API

- `GET /produtos` - Lista os produtos cadastrados.
- `POST /produtos` - Cadastra um novo produto e valida códigos duplicados.
- `PUT /produtos/{codigo}/baixar-estoque` - Realiza a baixa da quantidade informada no estoque do produto.

### Faturamento API

- `GET /notas` - Lista as notas fiscais com seus respectivos itens.
- `GET /notas/{id}` - Consulta uma nota fiscal específica.
- `POST /notas` - Cria uma nova nota com status `Aberta` e gera sua numeração sequencial.
- `POST /notas/{id}/imprimir` - Fecha uma nota aberta e solicita ao microsserviço de Estoque a baixa dos produtos.

## Tratamento de falhas

A comunicação entre os microsserviços possui tratamento de erros.

Caso o serviço de Estoque esteja indisponível durante o fechamento de uma nota, o serviço de Faturamento não conclui a operação. A nota permanece com status `Aberta` e a interface informa ao usuário que não foi possível realizar o fechamento.

Também são tratadas situações como estoque insuficiente, tentativa de criação de nota sem itens e dados obrigatórios não preenchidos.

## Decisões técnicas

- O sistema foi separado em dois microsserviços para manter independentes as responsabilidades de Estoque e Faturamento.
- Cada microsserviço possui seu próprio banco de dados PostgreSQL.
- O Entity Framework Core é utilizado para persistência e acesso aos dados.
- LINQ é utilizado nas consultas ao banco, como na busca de produtos e na geração sequencial do número das notas.
- A comunicação entre Faturamento e Estoque é realizada via HTTP utilizando `HttpClient`.
- No frontend, o `HttpClient` do Angular é utilizado para consumir as APIs, trabalhando com Observables do RxJS e `subscribe` para tratamento das respostas.
- Angular Signals são utilizados para controlar estados da interface, como produtos, notas, mensagens e processamento.

## Fluxo de fechamento da nota

1. O usuário cria uma nota fiscal.
2. A nota é criada com status `Aberta`.
3. O usuário solicita o fechamento.
4. A interface solicita confirmação.
5. O sistema apresenta o estado `Processando...`.
6. O serviço de Faturamento solicita a baixa ao serviço de Estoque.
7. Com a operação concluída, a nota passa para `Fechada`.
8. A interface atualiza as notas e os saldos dos produtos.

Caso o estoque esteja indisponível ou não seja suficiente, a operação é interrompida e a nota permanece aberta.

## Autor

Vinicius Alves Oliveira da Costa