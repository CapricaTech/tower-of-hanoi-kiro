# Design Document

## Overview

Este documento descreve o design técnico do jogo **Tower of Hanoi** em **.NET Framework 4.8** (`net48`), executável em modo texto no macOS via **Mono** (nativo ou em container Linux). O jogo é uma aplicação de console interativa com renderização ASCII colorida (ANSI), suporte a modo manual e auto execução, e uma arquitetura em camadas que separa o domínio (regras do jogo) da apresentação (console I/O), preparando o código para uma futura modernização.

### Objetivos de design

- **Separação de camadas**: domínio puro, sem dependência de console, testável e determinístico (Requirements 6, 10).
- **Portabilidade**: sem APIs Windows-only; cor via ANSI; roda idêntico em Mono local e container (Requirements 6, 7, 8, 9).
- **Baixo atrito de modernização**: domínio isolado facilita migração futura para .NET 8/9 (Requirement 6).
- **Robustez de entrada/saída**: validação de input, detecção de suporte a cor com fallback (Requirements 3, 9).

### Decisões de design (confirmadas)

- **Target**: `net48` (cenário de legado a modernizar).
- **Entrada de movimentos (modo manual)**: por rótulos de pino — o jogador digita origem e destino como `A`, `B` ou `C` (ex.: `A C`). Consistente com os rótulos exibidos no tabuleiro.
- **Discos**: inteiro de 4 a 8.
- **Cor**: códigos ANSI, com detecção de suporte e fallback monocromático; respeita `NO_COLOR`.

## Architecture

O sistema é organizado em três camadas mais um ponto de entrada:

```
+------------------------------------------------------------+
|                        Program (Main)                       |
|   composição de dependências + loop principal do app        |
+------------------------------------------------------------+
                 |                         |
                 v                         v
+---------------------------+   +----------------------------+
|      Presentation         |   |         Application         |
|  (Console I/O, ANSI)      |   |   (orquestração do jogo)    |
|  - IRenderer / Renderer   |   |   - GameController          |
|  - IInputReader / Input   |   |   - GameMode (Manual/Auto)  |
|  - AnsiConsole/Color      |   |   - AutoSolverRunner        |
+---------------------------+   +----------------------------+
                 |                         |
                 +-----------+-------------+
                             v
                +----------------------------+
                |          Domain             |
                |   (regras puras, testável)  |
                |   - Board                   |
                |   - Peg                     |
                |   - Move / MoveResult       |
                |   - HanoiSolver             |
                |   - GameState               |
                +----------------------------+
```

### Fluxo de alto nível

1. `Program.Main` compõe as dependências (renderer, input reader, controller) e inicia o loop.
2. `GameController` conduz: pergunta número de discos → pergunta modo → inicializa `Board` → executa loop de jogo (manual ou auto) → detecta vitória → oferece reiniciar/sair.
3. No **modo manual**, o controller lê comandos de teclado, aplica movimentos no `Board`, e pede ao `Renderer` para redesenhar.
4. No **modo auto**, o `HanoiSolver` gera a sequência ótima; o `AutoSolverRunner` aplica um movimento por vez com pausa, redesenhando entre passos, com possibilidade de interrupção.

### Projetos / estrutura de solução

```
TowerOfHanoi.sln
src/
  TowerOfHanoi/                 (projeto console, net48, exe)
    Program.cs
    Application/
      GameController.cs
      GameMode.cs
      AutoSolverRunner.cs
      GameOptions.cs
    Presentation/
      IRenderer.cs
      ConsoleRenderer.cs
      IInputReader.cs
      ConsoleInputReader.cs
      AnsiColor.cs
      ColorSupport.cs
    Domain/
      Board.cs
      Peg.cs
      Move.cs
      MoveResult.cs
      GameState.cs
      HanoiSolver.cs
tests/
  TowerOfHanoi.Tests/           (projeto de testes, net48)
    BoardTests.cs
    HanoiSolverTests.cs
    MoveValidationTests.cs
Dockerfile
README.md
```

> Observação: os testes só serão criados/expandidos se solicitado. A estrutura acima os prevê para manter o domínio testável (Requirements 6.4, 10.6), mas a implementação de testes não é automática.

## Components and Interfaces

### Domain Layer

#### `Peg`
Representa um pino e sua pilha de discos.

- Estado: identificador (`A`, `B`, `C`) e uma pilha de inteiros (tamanhos de disco; menor no topo).
- Responsabilidades:
  - `int? Peek()` — tamanho do disco no topo, ou `null` se vazio.
  - `bool IsEmpty`
  - `bool CanAccept(int discSize)` — `true` se vazio ou se `discSize < Peek()`.
  - `int Pop()` / `void Push(int discSize)` — mutação de baixo nível (usada pelo `Board` após validação).
  - `IReadOnlyList<int> DiscsBottomToTop` — para renderização.

#### `PegId` (enum)
`A = 0, B = 1, C = 2`. Facilita mapeamento e evita strings mágicas.

#### `Move`
Valor imutável: `PegId From`, `PegId To`.

#### `MoveResult`
Resultado de uma tentativa de movimento:
- `bool Success`
- `MoveError Error` (enum: `None`, `SourceEmpty`, `LargerOnSmaller`, `SameSourceAndTarget`, `InvalidPeg`)
- Mensagem amigável derivada do erro (mapeada na camada de apresentação, não no domínio).

#### `Board`
Núcleo das regras (Requirements 3, 4).

- Estado: três `Peg` (A, B, C), `int DiscCount`, `int MoveCount`.
- Construtor: `Board(int discCount)` — valida `4 <= discCount <= 8`, empilha discos `discCount..1` no pino A (maior na base).
- Métodos:
  - `MoveResult TryMove(Move move)` — valida (origem não vazia, destino aceita, origem ≠ destino) e, se válido, transfere o disco e incrementa `MoveCount`. **Não** lança exceção para movimentos inválidos; retorna `MoveResult`.
  - `bool IsSolved()` — `true` quando todos os discos estão no pino C na ordem correta.
  - `int MinimumMoves => (1 << DiscCount) - 1` — 2^n - 1.
  - `GameState Snapshot()` — cópia imutável do estado para o renderer.

#### `HanoiSolver`
Geração da solução ótima (Requirement 10).

- `IEnumerable<Move> Solve(int discCount, PegId from, PegId to, PegId aux)` — algoritmo recursivo clássico, retornando a sequência de `Move`. Puro e determinístico, sem I/O. Implementado com `yield return` para permitir consumo passo a passo.
- Garante exatamente 2^n - 1 movimentos.

#### `GameState`
DTO imutável com o snapshot para renderização: discos por pino, `MoveCount`, `MinimumMoves`, `DiscCount`, `IsSolved`.

### Presentation Layer

#### `ColorSupport`
Detecção de suporte a cor (Requirement 9.4, 9.5).

- `bool IsColorEnabled` calculado uma vez:
  - `false` se `Console.IsOutputRedirected` (saída não é terminal).
  - `false` se a variável de ambiente `NO_COLOR` estiver definida.
  - `false` se `TERM` for `dumb` ou ausente em ambiente não interativo.
  - `true` caso contrário.

#### `AnsiColor`
Constantes de escape ANSI e helpers.

- Códigos de foreground/background (ex.: `\u001b[31m`), reset (`\u001b[0m`).
- `string Wrap(string text, string colorCode)` — aplica cor e reset (Requirement 9.6). Se cor desabilitada, retorna `text` inalterado.
- Paleta fixa mapeando tamanho de disco → cor consistente durante a partida (Requirement 9.2).

#### `IRenderer` / `ConsoleRenderer`
Responsável por desenhar o tabuleiro (Requirements 2, 9).

- `void RenderBoard(GameState state)`:
  - Calcula a largura máxima (`2 * DiscCount + 1`) para dimensionar a área de cada pino.
  - Desenha os pinos de cima para baixo: cada nível é uma linha com os três pinos lado a lado.
  - Cada disco é um bloco (ex.: caracteres de bloco/`=`) de largura `2 * size + 1`, centralizado sobre o pino, colorido conforme `AnsiColor` quando habilitado.
  - Pinos vazios mostram apenas o "mastro" (ex.: `|`) sobre a base.
  - Desenha rótulos `A B C`, a base do tabuleiro, e a linha de status: `Movimentos: X / mínimo: Y`.
- `void RenderMessage(string message)` / `void RenderError(string message)` — mensagens e erros.
- `void RenderWelcome()` / `void RenderRules()` — boas-vindas e regras (Requirements 1.1, 5.3).
- `void RenderVictory(GameState state, bool optimal)` — vitória e reconhecimento de solução ótima (Requirement 4).
- `void ClearScreen()` — limpeza entre frames (usa ANSI, com fallback).

#### `IInputReader` / `ConsoleInputReader`
Leitura e parsing de entrada de teclado (Requirements 3, 5).

- `int ReadDiscCount()` — lê e valida 4–8, repetindo em erro (Requirement 1.2, 1.3).
- `GameMode ReadGameMode()` — lê escolha manual/auto (Requirement 1.5, 1.6).
- `PlayerCommand ReadCommand()` — interpreta a entrada do turno como:
  - Um **movimento** (`A C`, `AC`, case-insensitive) → `Move`.
  - Um **comando de controle**: `q`/`sair`, `r`/`reiniciar`, `h`/`ajuda` (Requirement 5).
  - **Inválido** → sinaliza erro para o controller reexibir (Requirement 3.6).

#### `PlayerCommand`
Union simples: tipo (`Move`, `Quit`, `Restart`, `Help`, `Invalid`) + `Move` opcional.

### Application Layer

#### `GameMode` (enum)
`Manual`, `Auto`.

#### `GameOptions`
Opções resolvidas na inicialização: `bool ColorEnabled`, `int AutoStepDelayMs` (pausa da auto execução, Requirement 10.2).

#### `GameController`
Orquestra o ciclo de vida do jogo.

- `void Run()` — loop principal:
  1. `RenderWelcome` + `RenderRules`.
  2. `discCount = ReadDiscCount()`.
  3. `mode = ReadGameMode()`.
  4. `board = new Board(discCount)`.
  5. Se `Manual` → `RunManualLoop(board)`; se `Auto` → `AutoSolverRunner.Run(board)`.
  6. Ao vencer → `RenderVictory`; oferecer reiniciar/sair.
- `RunManualLoop(Board board)`:
  - Redesenha o tabuleiro; lê comando; aplica `Move` via `board.TryMove` (mostrando erro em falha); trata `Quit`/`Restart`/`Help`; verifica `IsSolved` após cada movimento.

#### `AutoSolverRunner`
Executa a auto execução (Requirement 10).

- `void Run(Board board)`:
  - `moves = HanoiSolver.Solve(board.DiscCount, A, C, B)`.
  - Para cada `move`: aplica `board.TryMove(move)`, `RenderBoard`, aguarda `AutoStepDelayMs`.
  - Checa interrupção do usuário (`Console.KeyAvailable` / tratamento de `Ctrl+C` via `Console.CancelKeyPress`) para parada limpa (Requirement 10.5).
  - Ao final: `IsSolved` deve ser `true` e `MoveCount == MinimumMoves` (Requirement 10.4).

## Data Models

### Estado do tabuleiro (em memória)

```
Board
  ├─ Peg A : Stack<int>   (ex.: base→topo: 5,4,3,2,1)
  ├─ Peg B : Stack<int>
  ├─ Peg C : Stack<int>
  ├─ DiscCount : int (4..8)
  └─ MoveCount : int
```

- Discos representados por inteiros iguais ao seu tamanho (1 = menor). Invariante: em cada pino, os valores crescem da topo para a base (topo é sempre o menor).

### GameState (snapshot para render)

```
GameState
  ├─ PegA : IReadOnlyList<int>  (base→topo)
  ├─ PegB : IReadOnlyList<int>
  ├─ PegC : IReadOnlyList<int>
  ├─ DiscCount : int
  ├─ MoveCount : int
  ├─ MinimumMoves : int
  └─ IsSolved : bool
```

### Layout de renderização

- Largura de um disco de tamanho `s`: `2*s + 1` caracteres.
- Largura da coluna de um pino: `2*DiscCount + 1` (comporta o maior disco).
- Altura da área de discos: `DiscCount` linhas.
- Espaçamento entre pinos: 2+ espaços para separação visual.

## Error Handling

- **Entrada inválida (discos/modo/movimento)**: nunca lança exceção para o usuário; o `InputReader` retorna um resultado tipado e o `GameController` reexibe mensagem de erro e repete a solicitação (Requirements 1.3, 1.6, 3.6).
- **Movimentos inválidos**: `Board.TryMove` retorna `MoveResult` com `MoveError`; a mensagem amigável é mapeada na apresentação. Estado permanece inalterado (Requirements 3.2, 3.3, 3.5).
- **Domínio**: construtor de `Board` lança `ArgumentOutOfRangeException` para `discCount` fora de 4–8 — mas essa condição é barrada antes pelo `InputReader`, então a exceção é uma salvaguarda de programação, não um caminho de usuário.
- **Terminal sem cor / saída redirecionada**: `ColorSupport` desativa ANSI e o render cai em modo texto puro, sem emitir escapes (Requirement 9.4).
- **Interrupção da auto execução / Ctrl+C**: capturada para restaurar o terminal (cor/estado) e sair limpo (Requirements 10.5).
- **Restauração do terminal**: ao encerrar, garante reset de cor (`\u001b[0m`) para não vazar atributos (Requirement 9.6).

## Testing Strategy

> Testes automatizados serão implementados apenas se solicitados. Esta seção define a abordagem recomendada para manter o domínio verificável.

### Testes de unidade (domínio — determinísticos, sem I/O)

- **BoardTests**: inicialização correta (discos em A, ordem certa); `TryMove` válido incrementa contador e move o disco; movimentos inválidos (origem vazia, maior sobre menor, origem=destino) são rejeitados e não alteram estado; `IsSolved` só é `true` com todos os discos em C; `MinimumMoves` = 2^n - 1.
- **HanoiSolverTests**: para n de 4 a 8, a sequência tem exatamente 2^n - 1 movimentos; aplicando a sequência a um `Board`, o resultado é `IsSolved`; cada movimento gerado é válido.
- **MoveValidationTests**: parsing de entrada (`A C`, `ac`, inválidos) e mapeamento para `Move`/comandos.

### Verificação de build e execução

- Compilar com `net48` via `msbuild` (Mono) ou `dotnet build`.
- Executar via `mono TowerOfHanoi.exe` (local) e via container (`docker build` + `docker run -it`).
- Testar manualmente: partida manual completa, auto execução até vitória, comandos de controle, fallback sem cor (`NO_COLOR=1` e saída redirecionada).

## Deployment / Execução

### Mono local (Requirement 7)

```
# instalar mono (ex.: brew install mono)
msbuild /p:Configuration=Release          # ou: dotnet build -c Release
mono src/TowerOfHanoi/bin/Release/TowerOfHanoi.exe
```

### Container (Requirement 8)

`Dockerfile` multi-stage baseado em `mono:latest` (imagem Linux):

- **Stage build**: usa `mono` + `msbuild`/`nuget` para restaurar e compilar em Release.
- **Stage runtime**: copia o `.exe` e assemblies, define `ENTRYPOINT ["mono", "TowerOfHanoi.exe"]`.

```
docker build -t tower-of-hanoi .
docker run -it --rm tower-of-hanoi
```

- Em Apple Silicon, se necessário: `docker run --platform linux/amd64 -it --rm tower-of-hanoi` (Requirement 8.4).
- O mesmo executável serve para Mono local e container, sem alteração de código (Requirements 7.5, 8.7).
