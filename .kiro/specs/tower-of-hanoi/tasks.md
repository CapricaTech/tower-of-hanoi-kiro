# Implementation Plan

- [ ] 1. Criar a estrutura da solução e do projeto console
  - Criar `TowerOfHanoi.sln` e o projeto `src/TowerOfHanoi/TowerOfHanoi.csproj` como executável de console com target `net48`.
  - Definir a estrutura de pastas `Domain/`, `Presentation/`, `Application/` e o `Program.cs` inicial (apenas um "Hello" temporário para validar o build).
  - Validar que o projeto compila (`msbuild`/`dotnet build`) e roda via `mono` no macOS.
  - _Requirements: 6.3, 7.1, 7.2, 7.3, 7.4_

- [ ] 2. Implementar o modelo de domínio de pinos e movimentos
- [ ] 2.1 Implementar `PegId`, `Move`, `MoveError` e `MoveResult`
  - Criar o enum `PegId` (A, B, C) e o value object imutável `Move` (From, To).
  - Criar o enum `MoveError` (None, SourceEmpty, LargerOnSmaller, SameSourceAndTarget, InvalidPeg) e o tipo `MoveResult` (Success + Error).
  - _Requirements: 3.1, 3.2, 3.3, 3.5, 3.6_

- [ ] 2.2 Implementar `Peg`
  - Implementar a pilha de discos com `Peek`, `IsEmpty`, `CanAccept(discSize)`, `Push`, `Pop` e `DiscsBottomToTop`.
  - Garantir a invariante de ordenação (topo é sempre o menor).
  - _Requirements: 2.1, 3.2, 3.3_

- [ ] 3. Implementar o `Board` com regras e estado
- [ ] 3.1 Implementar inicialização e estado do tabuleiro
  - Construtor `Board(int discCount)` validando 4–8 e empilhando discos no pino A (maior na base).
  - Expor `DiscCount`, `MoveCount` e `MinimumMoves` (2^n - 1).
  - _Requirements: 1.2, 1.4, 2.3, 4.2_

- [ ] 3.2 Implementar `TryMove` e detecção de vitória
  - `TryMove(Move)` validando origem não vazia, destino aceita disco e origem ≠ destino; em sucesso, transfere o disco e incrementa `MoveCount`; em falha, retorna `MoveResult` sem alterar estado.
  - `IsSolved()` verdadeiro apenas com todos os discos no pino C na ordem correta.
  - `Snapshot()` retornando `GameState` imutável.
  - _Requirements: 3.2, 3.3, 3.4, 3.5, 4.1_

- [ ] 4. Implementar o `HanoiSolver` (solução ótima)
  - Implementar `Solve(discCount, from, to, aux)` com o algoritmo recursivo clássico usando `yield return`, puro e determinístico.
  - Garantir exatamente 2^n - 1 movimentos e que cada movimento gerado seja válido.
  - _Requirements: 10.1, 10.4, 10.6_

- [ ] 5. Implementar a camada de cor ANSI e detecção de suporte
- [ ] 5.1 Implementar `ColorSupport`
  - Detectar suporte a cor: desativar se `Console.IsOutputRedirected`, se `NO_COLOR` estiver definido, ou se `TERM` indicar terminal sem cor.
  - _Requirements: 9.4, 9.5_

- [ ] 5.2 Implementar `AnsiColor`
  - Definir códigos de escape ANSI (foreground, reset) e `Wrap(text, color)` que aplica cor + reset, ou retorna texto puro quando cor desabilitada.
  - Definir paleta fixa mapeando tamanho de disco → cor consistente.
  - _Requirements: 9.2, 9.3, 9.6_

- [ ] 6. Implementar o `ConsoleRenderer`
- [ ] 6.1 Implementar a renderização do tabuleiro
  - Desenhar os três pinos lado a lado com discos como blocos ASCII de largura `2*size+1`, centralizados, com base e rótulos A/B/C.
  - Aplicar cor por disco quando habilitada; pinos vazios mostram apenas o mastro.
  - Exibir a linha de status com movimentos atuais e mínimo (2^n - 1).
  - _Requirements: 2.1, 2.2, 2.3, 9.1, 9.2, 9.3_

- [ ] 6.2 Implementar mensagens, boas-vindas, regras e vitória
  - `RenderWelcome`, `RenderRules`, `RenderMessage`, `RenderError`, `ClearScreen`.
  - `RenderVictory` exibindo total de movimentos, mínimo, e reconhecimento de solução ótima.
  - Garantir reset de cor ao final para não vazar atributos.
  - _Requirements: 1.1, 4.2, 4.3, 5.3, 9.6_

- [ ] 7. Implementar o `ConsoleInputReader` e parsing de comandos
- [ ] 7.1 Implementar leitura de número de discos e modo de jogo
  - `ReadDiscCount()` validando 4–8 e repetindo em erro.
  - `ReadGameMode()` lendo manual/auto e repetindo em erro.
  - _Requirements: 1.2, 1.3, 1.5, 1.6_

- [ ] 7.2 Implementar `ReadCommand` e `PlayerCommand`
  - Interpretar entrada como movimento por rótulos de pino (`A C`, `ac`, case-insensitive), ou comandos `q`/`sair`, `r`/`reiniciar`, `h`/`ajuda`, ou inválido.
  - _Requirements: 3.1, 3.6, 5.1, 5.2, 5.3_

- [ ] 8. Implementar a camada de aplicação (orquestração)
- [ ] 8.1 Implementar `GameMode`, `GameOptions` e o esqueleto do `GameController`
  - Definir `GameMode` (Manual/Auto) e `GameOptions` (ColorEnabled, AutoStepDelayMs).
  - Implementar `Run()` conduzindo boas-vindas → discos → modo → inicialização do `Board` → despacho para loop manual ou auto → vitória → reiniciar/sair.
  - _Requirements: 1.1, 1.4, 1.5, 4.1, 5.2_

- [ ] 8.2 Implementar o loop de jogo manual
  - Redesenhar tabuleiro, ler comando, aplicar movimento (mostrando erro em falha), tratar sair/reiniciar/ajuda, e checar vitória após cada movimento.
  - _Requirements: 2.1, 3.2, 3.3, 3.4, 3.5, 3.6, 4.1, 5.1, 5.2, 5.3_

- [ ] 8.3 Implementar o `AutoSolverRunner`
  - Consumir a sequência do `HanoiSolver`, aplicar um movimento por vez com pausa configurável, redesenhando entre passos.
  - Permitir interrupção limpa (tecla / `Ctrl+C`) restaurando o terminal.
  - Ao final, garantir `IsSolved` e `MoveCount == MinimumMoves` e declarar vitória.
  - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5_

- [ ] 9. Compor as dependências no `Program.Main`
  - Instanciar `ColorSupport`, `AnsiColor`, `ConsoleRenderer`, `ConsoleInputReader`, `GameController` e iniciar o loop.
  - Registrar handler de `Console.CancelKeyPress` para restaurar cor/estado do terminal na saída.
  - _Requirements: 5.1, 9.6, 10.5_

- [ ] 10. Criar o `Dockerfile` para execução em container
  - Criar `Dockerfile` multi-stage baseado em imagem Linux com Mono: stage de build compila em Release; stage de runtime copia o `.exe` e define `ENTRYPOINT ["mono", "TowerOfHanoi.exe"]`.
  - Validar `docker build` e `docker run -it`, incluindo o caso Apple Silicon (`--platform linux/amd64` quando necessário).
  - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.6, 8.7_

- [ ] 11. Escrever a documentação de build e execução no README
  - Documentar instalação do Mono, compilação e execução local via `mono TowerOfHanoi.exe`.
  - Documentar build e execução via container, e as regras/comandos do jogo.
  - Deixar claro que Mono local e container coexistem como opções.
  - _Requirements: 7.1, 7.5, 8.5, 8.7_

- [ ] 12. Verificação integrada final
  - Compilar via `msbuild`/`dotnet build` e rodar via `mono` localmente; validar partida manual completa, auto execução até a vitória, e comandos de controle.
  - Validar fallback sem cor (`NO_COLOR=1` e saída redirecionada) sem exibir caracteres de controle crus.
  - Validar a execução em container em modo interativo.
  - _Requirements: 4.1, 5.1, 7.3, 8.3, 9.4, 10.4_
