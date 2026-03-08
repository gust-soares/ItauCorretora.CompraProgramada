# 🏦 Itaú Corretora - API de Compra Programada & Wealth Management

Este projeto é uma API desenvolvida em **.NET Core** para simular o sistema de Gestão de Patrimônio (Wealth Management) da Itaú Corretora. O sistema permite a adesão de clientes a um plano de "Compra Programada", executando investimentos mensais automatizados baseados em uma "Cesta Top Five" recomendada.

Além disso, a API realiza o **rebalanceamento dinâmico da carteira** e a apuração de Imposto de Renda (IR), integrando-se via mensageria para o envio de notificações (Dedo Duro).

---

## 🏗️ Arquitetura Utilizada

O projeto foi construído utilizando os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, visando o baixo acoplamento, alta coesão, manutenibilidade e facilidade para testes unitários.

A solução está dividida nas seguintes camadas:

- **Domain:** O núcleo do sistema. Contém as Entidades de negócio (`Cliente`, `ContaGrafica`, `CustodiaFilhote`, `CestaTopFive`) e as Interfaces de Repositório. É totalmente isolada de frameworks externos.
- **Application:** Contém os Casos de Uso (`UseCases`) e os `DTOs`. Orquestra a lógica de negócio, como os cálculos matemáticos de `InvestimentoMensal`, apuração de `Lucro/Prejuízo` (Mark-to-Market) e a lógica de `Rebalanceamento`.
- **Infrastructure:** Responsável pela comunicação com agentes externos e persistência. Contém a implementação do acesso a dados via **Entity Framework Core (MySQL)**, a leitura posicional do arquivo de cotações da B3 (`B3ParserService`) e a mensageria (`KafkaService`).
- **Api (Presentation):** Os `Controllers` que expõem os endpoints REST documentados via Swagger. Eles recebem as requisições HTTP e repassam os comandos para a camada de *Application*.

---

## 💡 Decisões Técnicas

- **Banco de Dados Relacional (MySQL):** Escolhido para garantir consistência, integridade referencial e suporte a transações (ACID) — requisitos fundamentais para sistemas financeiros que lidam com saldos e custódia de ativos.
- **Mensageria com Apache Kafka:** O requisito de notificação do "Dedo Duro" do Imposto de Renda (operações de venda superiores a R$ 20.000,00 com lucro) foi implementado via eventos assíncronos. O Kafka foi escolhido por sua alta resiliência, performance e capacidade de desacoplar o motor principal de investimentos do serviço de notificação fiscal.
- **Processamento de Arquivos B3 (COTAHIST):** O parsing do arquivo TXT da B3 foi construído utilizando leitura de strings por posição fixa (`Substring`) e higienização com `.Trim()`. Isso garante a extração precisa de Tickers e Preços de Fechamento com alta performance e tolerância a falhas de formatação.
- **Containerização (Docker):** Toda a infraestrutura da aplicação (API, MySQL, Zookeeper e Kafka) foi encapsulada no `docker-compose`. Isso permite que qualquer desenvolvedor ou avaliador suba a infraestrutura completa da aplicação com um único comando.

---

## 🛠️ Pré-requisitos

Para executar o projeto localmente, você precisará apenas do Docker instalado e rodando em sua máquina:

- [Docker Desktop](https://www.docker.com/products/docker-desktop)

> **Nota:** O SDK do .NET não é estritamente necessário para executar o projeto, pois o build da API é feito internamente pelo Dockerfile.

---

## 🚀 Como Executar o Projeto

1. Clone este repositório para a sua máquina local.
2. Certifique-se de ter um arquivo válido de cotações da B3 (ex: `COTAHIST_D05032026.TXT`) dentro da pasta `cotacoes` na raiz do projeto.
3. Abra o terminal na raiz do projeto (onde o arquivo `docker-compose.yml` está localizado) e execute:

```bash
docker-compose up -d --build
```

4. Aguarde a inicialização dos contêineres (MySQL e Kafka podem levar alguns instantes para ficarem prontos).
5. Acesse a documentação interativa da API (Swagger) pelo navegador:

👉 **http://localhost:7100/swagger**

6. Acesse o frontend com visualização gráfica por cliente:

👉 **http://localhost:7100**

---

## 🗺️ Guia de Testes de Ponta a Ponta

Siga este roteiro no Swagger para simular o ciclo de vida completo de um investidor e testar todas as funcionalidades:

### 1. Configuração Inicial e Adesão

**Cadastrar Cesta Top Five:** `POST /api/Admin/cesta`

Cadastre a recomendação inicial da corretora com 5 ativos de peso igual.

```json
[
  {"ticker": "ITUB4", "percentual": 20},
  {"ticker": "VALE3", "percentual": 20},
  {"ticker": "PETR4", "percentual": 20},
  {"ticker": "BBDC4", "percentual": 20},
  {"ticker": "ABEV3", "percentual": 20}
]
```

**Adesão do Cliente:** `POST /api/Clientes/adesao`

Cadastre um cliente informando seus dados básicos e o `valorMensalAporte` (ex: R$ 5.000,00). Guarde o ID (Guid) retornado.

---

### 2. O Motor de Investimento

**Executar Investimento Mensal:** `POST /api/Processamento/executar-investimento-mensal`

Envie a requisição passando `/app/cotacoes` como caminho (o Docker fará a leitura da pasta mapeada).

O motor calculará a quantidade ideal de cotas proporcionais à Cesta Top Five ativa, com base no último preço de mercado.

---

### 3. Acompanhamento da Custódia

**Consultar Posição da Carteira:** `GET /api/Carteira/{clienteId}/posicao`

Insira o ID do cliente gerado no passo 1. O retorno incluirá:

- Quantidade de ativos e Preço Médio
- Preço Atual de Mercado (consumido do TXT)
- Lucro/Prejuízo total e percentual de Rentabilidade

---

### 4. Rebalanceamento Dinâmico e Integração Kafka

**Alterar a Estratégia de Investimento:** `POST /api/Admin/cesta`

Altere os pesos para tornar a carteira mais agressiva. Exemplo: PETR4 com 80% e os demais com 5%.

**Executar o Rebalanceamento:** `POST /api/Processamento/executar-rebalanceamento`

O sistema identificará os ativos que ultrapassaram o limite percentual estipulado pela nova cesta, executará ordens de **Venda** e em seguida ordens de **Compra** para o ativo alvo.

**Validação do Dedo Duro:**

Se o valor total das vendas no mês ultrapassar R$ 20.000,00 e houver lucro apurado, acompanhe os logs do contêiner:

```bash
docker logs itau_api
```

Você verá o registro do disparo do evento fiscal para o tópico do Kafka.