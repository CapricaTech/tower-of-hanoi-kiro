# tower-of-hanoi-kiro

Jogo **Torre de Hanoi** em modo texto, escrito em **.NET Framework 4.8** (`net48`)
e executado no **macOS** via **Mono** — localmente ou em **container**.

O alvo `net48` foi mantido intencionalmente para representar uma base de codigo
legada que sera modernizada depois (por exemplo, migracao para .NET 8/9). Por
isso o jogo e um aplicativo de console, sem dependencias especificas do Windows,
com a logica de dominio separada da camada de apresentacao.

## Recursos

- Dois estilos de tabuleiro, escolhidos no inicio: **ASCII** (discos com `=`) ou
  **blocos** (discos solidos com caracteres de bloco Unicode `█`, desenhados no
  dobro do tamanho — 2x em largura e altura).
- Cores via sequencias de escape ANSI, com deteccao automatica e _fallback_
  monocromatico quando o terminal nao suporta cor ou a saida e redirecionada.
- Modo **manual** (voce move os discos) e modo de **auto execucao** (o computador
  resolve o quebra-cabeca com a solucao otima, passo a passo).
- De 4 a 8 discos.

## Requisitos

- [Mono](https://www.mono-project.com/) instalado (fornece `mcs` e `mono`).
  No macOS: `brew install mono`.
- Opcional, para o modo container: Docker (ou Podman).

## Inicio rapido

Escolha um dos dois caminhos abaixo.

**Com Mono local** (precisa de `mono`/`mcs` instalados):

```bash
./build.sh
mono src/TowerOfHanoi/bin/Release/TowerOfHanoi.exe
```

**Com container** (precisa de Docker; nao precisa de Mono na maquina):

```bash
docker build -t tower-of-hanoi .
docker run -it --rm tower-of-hanoi
```

Detalhes de cada caminho estao nas secoes [Execucao local com Mono](#execucao-local-com-mono)
e [Execucao como container](#execucao-como-container) abaixo.

## Como jogar

Ao iniciar, o jogo pergunta, nesta ordem: o **estilo do tabuleiro**
(`a` = ASCII ou `b` = blocos), o **numero de discos** (4 a 8) e o **modo**
(manual ou auto).

No modo manual, informe o movimento como origem e destino usando os rotulos dos
pinos. Exemplos equivalentes: `A C`, `a c`, `ac`.

Comandos disponiveis durante a partida:

| Comando            | Acao                       |
| ------------------ | -------------------------- |
| `A C` (ou `ac`)    | mover do pino A para o C   |
| `h` ou `ajuda`     | mostrar as regras          |
| `r` ou `reiniciar` | recomecar a partida        |
| `q` ou `sair`      | sair do jogo               |

Regra principal: um disco maior nunca pode ser colocado sobre um disco menor.
O objetivo e transferir toda a pilha para o pino **C**.

### Desabilitar cores

O jogo respeita a convencao [`NO_COLOR`](https://no-color.org) e tambem aceita a
flag `--no-color`:

```bash
NO_COLOR=1 mono TowerOfHanoi.exe
# ou
mono TowerOfHanoi.exe --no-color
```

## Execucao local com Mono

Compile com o script fornecido (usa o compilador do Mono, `mcs`, mirando o
perfil 4.8) e execute o `.exe` com `mono`:

```bash
# compilar (Release por padrao; use ./build.sh Debug para Debug)
./build.sh

# executar
mono src/TowerOfHanoi/bin/Release/TowerOfHanoi.exe
```

> Observacao: o arquivo `TowerOfHanoi.csproj` (target `net48`) e mantido como o
> artefato de projeto para a futura modernizacao. Na maioria dos ambientes macOS
> o caminho de build confiavel e o `build.sh` acima, pois o _targeting pack_ do
> .NET Framework para o MSBuild nao costuma estar instalado.

## Execucao como container

A execucao em container e uma alternativa opcional a execucao local com Mono —
util para um ambiente isolado e reproduzivel. Ambos os modos usam o mesmo
executavel, sem alteracao de codigo.

> A imagem e baseada em **Linux + Mono**. Containers Windows com o .NET Framework
> real nao rodam no macOS e, por isso, nao sao usados aqui.

```bash
# construir a imagem
docker build -t tower-of-hanoi .

# executar em modo interativo (necessario para jogar pelo terminal)
docker run -it --rm tower-of-hanoi
```

Em Macs com Apple Silicon, se necessario, force a plataforma:

```bash
docker run --platform linux/amd64 -it --rm tower-of-hanoi
```

## Estrutura do projeto

```
TowerOfHanoi.sln
build.sh                     # build via mcs (Mono) para net48
Dockerfile                   # imagem Linux + Mono (multi-stage)
src/TowerOfHanoi/
  Program.cs                 # composicao de dependencias + Ctrl+C
  Domain/                    # regras puras, testaveis (sem console)
    Board, Peg, Move, MoveResult, GameState, HanoiSolver, ...
  Presentation/              # console I/O e cor ANSI
    ConsoleRenderer, ConsoleInputReader, AnsiColor, ColorSupport, ...
  Application/               # orquestracao do jogo
    GameController, AutoSolverRunner, GameMode, GameOptions
```

A camada **Domain** nao depende de console nem de APIs do Windows, o que mantem
a logica testavel e facilita a futura modernizacao para .NET moderno.
