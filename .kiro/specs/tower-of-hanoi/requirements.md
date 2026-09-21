# Requirements Document

## Introdução

Este documento descreve os requisitos para um jogo **Tower of Hanoi (Torre de Hanói)** implementado em **.NET Framework 4.8**, com o objetivo de rodar localmente no **macOS** (via Mono) e servir como base de código legada para uma futura modernização/atualização (por exemplo, migração para .NET 8/9).

O jogo é uma aplicação de **console interativa** que permite ao jogador mover discos entre três pinos, respeitando as regras clássicas do quebra-cabeça, com o objetivo de transferir toda a pilha de discos de um pino de origem para um pino de destino no menor número possível de movimentos.

### Contexto técnico e restrições

- **Target framework**: .NET Framework 4.8 (`net48`), mantido intencionalmente para representar um cenário de código legado a ser modernizado posteriormente.
- **Execução no macOS**: como o .NET Framework 4.8 é Windows-only, a execução local no macOS será feita via **Mono** (`mono TowerOfHanoi.exe`), seja com Mono instalado nativamente ou dentro de um **container Linux baseado em Mono**. Por isso a aplicação deve ser **console** e **não** usar APIs Windows-only (WinForms, WPF, Registry, etc.).
- **Containerização**: a execução via container deve usar uma imagem Linux com Mono (ex.: `mono:latest`), pois containers Windows com o .NET Framework real não rodam no macOS. O mesmo executável serve para execução via Mono nativo e via container, sem alterações de código.
- **Portabilidade para modernização**: a lógica do jogo deve ser separada da camada de interface (console), de modo que a migração futura para .NET moderno ou para uma UI gráfica seja de baixo atrito.

## Requirements

### Requirement 1 — Iniciar uma nova partida

**User Story:** Como jogador, quero iniciar uma nova partida escolhendo o número de discos, para que eu possa ajustar a dificuldade do quebra-cabeça.

#### Acceptance Criteria

1. QUANDO o jogo é iniciado ENTÃO o sistema DEVE exibir uma mensagem de boas-vindas e uma breve explicação das regras.
2. QUANDO o jogo solicita o número de discos ENTÃO o sistema DEVE aceitar um valor inteiro entre 4 e 8 (inclusive).
3. SE o jogador informar um valor fora do intervalo permitido ou não numérico ENTÃO o sistema DEVE exibir uma mensagem de erro e solicitar a entrada novamente.
4. QUANDO o número de discos é definido ENTÃO o sistema DEVE posicionar todos os discos empilhados no primeiro pino (A), do maior (base) ao menor (topo).
5. QUANDO o jogo é iniciado ENTÃO o sistema DEVE perguntar ao jogador qual o estilo de renderização do tabuleiro: **ASCII** (blocos com o caractere `=`, o estilo atual) ou **blocos** (blocos sólidos com caracteres de bloco Unicode).
6. SE a escolha do estilo de renderização for inválida ENTÃO o sistema DEVE exibir uma mensagem de erro e solicitar a escolha novamente.
7. QUANDO o número de discos é definido ENTÃO o sistema DEVE perguntar ao jogador qual o modo de jogo: **manual** (o jogador faz os movimentos) ou **auto execução** (o computador resolve o quebra-cabeça automaticamente).
8. SE a escolha do modo for inválida ENTÃO o sistema DEVE exibir uma mensagem de erro e solicitar a escolha novamente.

### Requirement 2 — Visualizar o estado do tabuleiro

**User Story:** Como jogador, quero ver o estado atual dos três pinos e seus discos, para que eu possa decidir meu próximo movimento.

#### Acceptance Criteria

1. QUANDO o tabuleiro é exibido ENTÃO o sistema DEVE mostrar os três pinos rotulados (A, B, C) com seus discos em representação textual (ASCII), desenhando os pinos e a base do tabuleiro.
2. QUANDO um disco é exibido ENTÃO o sistema DEVE representar seu tamanho relativo de forma visualmente distinguível (discos maiores mais largos que discos menores), usando blocos ASCII com largura proporcional ao tamanho do disco.
3. QUANDO o tabuleiro é exibido ENTÃO o sistema DEVE mostrar o contador de movimentos atual e o número mínimo de movimentos necessário (2^n - 1).
4. QUANDO o tabuleiro é exibido em um terminal com suporte a cor ENTÃO o sistema DEVE renderizar os discos como blocos ASCII coloridos, com uma cor distinta por disco, para facilitar a leitura visual.

### Requirement 3 — Realizar um movimento

**User Story:** Como jogador, quero mover o disco do topo de um pino para outro, para que eu possa progredir na resolução do quebra-cabeça.

#### Acceptance Criteria

1. QUANDO o jogador informa um movimento ENTÃO o sistema DEVE aceitar a origem e o destino como rótulos de pino (A, B ou C).
2. SE o pino de origem estiver vazio ENTÃO o sistema DEVE rejeitar o movimento e exibir uma mensagem de erro.
3. SE o movimento colocar um disco maior sobre um disco menor ENTÃO o sistema DEVE rejeitar o movimento e exibir uma mensagem de erro, mantendo o estado inalterado.
4. QUANDO um movimento válido é realizado ENTÃO o sistema DEVE mover o disco do topo da origem para o topo do destino e incrementar o contador de movimentos em 1.
5. SE o jogador informar origem e destino iguais ENTÃO o sistema DEVE rejeitar o movimento e exibir uma mensagem de erro.
6. SE a entrada do movimento for inválida (formato incorreto ou pino inexistente) ENTÃO o sistema DEVE exibir uma mensagem de erro e solicitar novamente sem alterar o estado.

### Requirement 4 — Vencer a partida

**User Story:** Como jogador, quero ser reconhecido quando resolver o quebra-cabeça, para que eu saiba que venci e com que desempenho.

#### Acceptance Criteria

1. QUANDO todos os discos estiverem empilhados corretamente no pino de destino (C) ENTÃO o sistema DEVE declarar a vitória.
2. QUANDO a vitória é declarada ENTÃO o sistema DEVE exibir o total de movimentos usados e o número mínimo de movimentos possível.
3. SE o jogador resolver no número mínimo de movimentos ENTÃO o sistema DEVE exibir uma mensagem de reconhecimento de solução ótima.

### Requirement 5 — Comandos de controle da partida

**User Story:** Como jogador, quero comandos para reiniciar ou sair do jogo, para que eu possa controlar minha sessão.

#### Acceptance Criteria

1. QUANDO o jogador digita o comando de sair (por exemplo, `q` ou `sair`) ENTÃO o sistema DEVE encerrar o jogo de forma limpa exibindo uma mensagem de despedida.
2. QUANDO o jogador digita o comando de reiniciar (por exemplo, `r` ou `reiniciar`) ENTÃO o sistema DEVE recomeçar a partida solicitando novamente o número de discos.
3. QUANDO o jogador digita um comando de ajuda (por exemplo, `h` ou `ajuda`) ENTÃO o sistema DEVE reexibir as regras e o formato de entrada dos comandos.

### Requirement 6 — Arquitetura preparada para modernização

**User Story:** Como mantenedor, quero que a lógica do jogo seja independente da interface de console, para que a futura atualização/migração seja de baixo atrito.

#### Acceptance Criteria

1. QUANDO o código é organizado ENTÃO o sistema DEVE separar a lógica de domínio (estado dos pinos, regras de movimento, condição de vitória) da camada de apresentação (entrada/saída de console).
2. QUANDO a lógica de domínio é implementada ENTÃO o sistema NÃO DEVE depender de APIs específicas do Windows (WinForms, WPF, Registry, `System.Drawing` etc.).
3. QUANDO o projeto é compilado ENTÃO o sistema DEVE ter como alvo o `net48` e produzir um executável que rode via Mono no macOS.
4. QUANDO a lógica de domínio expõe operações ENTÃO ela DEVE fazê-lo por métodos testáveis e determinísticos, sem efeitos colaterais de console.

### Requirement 7 — Execução local com Mono no macOS

**User Story:** Como desenvolvedor no macOS, quero rodar o jogo diretamente com o Mono instalado localmente, para que eu possa executá-lo sem precisar de container.

#### Acceptance Criteria

1. QUANDO o repositório é entregue ENTÃO o sistema DEVE incluir documentação (README) com os passos para instalar o Mono e compilar/executar o jogo no macOS.
2. QUANDO o jogo é compilado no macOS ENTÃO o sistema DEVE ser construído usando ferramentas disponíveis no macOS (por exemplo, `msbuild` do Mono ou `dotnet build` com target `net48`).
3. QUANDO o jogo é executado ENTÃO o sistema DEVE funcionar em um terminal padrão do macOS sem dependências gráficas.
4. QUANDO o executável é gerado ENTÃO o sistema DEVE poder ser iniciado localmente via `mono TowerOfHanoi.exe`, sem necessidade de container.
5. O suporte à execução local com Mono DEVE ser mantido em paralelo à execução em container (Requirement 8); ambos os caminhos DEVEM permanecer funcionais e documentados, sem que um substitua o outro.

### Requirement 8 — Execução como container

**User Story:** Como desenvolvedor no macOS, quero rodar o jogo dentro de um container, para que eu tenha um ambiente de execução isolado e reproduzível sem depender de uma instalação local do Mono.

#### Acceptance Criteria

1. QUANDO o repositório é entregue ENTÃO o sistema DEVE incluir um `Dockerfile` baseado em uma imagem Linux com Mono (por exemplo, `mono:latest`).
2. QUANDO a imagem é construída ENTÃO o sistema DEVE compilar (ou copiar) o executável do jogo e defini-lo como ponto de entrada via `mono TowerOfHanoi.exe`.
3. QUANDO o container é executado em modo interativo (`docker run -it`) ENTÃO o sistema DEVE permitir jogar normalmente pela entrada e saída padrão do terminal.
4. QUANDO o container é usado no macOS ENTÃO o sistema DEVE funcionar tanto em Mac Intel quanto Apple Silicon, aceitando emulação de plataforma quando necessário.
5. QUANDO a documentação é entregue ENTÃO o sistema DEVE incluir os comandos para construir a imagem e executar o container.
6. O sistema NÃO DEVE depender de containers Windows, pois estes não são executáveis no macOS.
7. A execução em container DEVE ser uma alternativa opcional à execução local com Mono (Requirement 7), e não uma substituição; o mesmo executável DEVE servir aos dois modos sem alterações de código.

### Requirement 9 — Renderização visual em modo texto com blocos ASCII coloridos

**User Story:** Como jogador, quero que os discos sejam desenhados como blocos ASCII coloridos, para que o tabuleiro fique mais legível e agradável em modo texto.

#### Acceptance Criteria

1. QUANDO o jogo desenha o tabuleiro ENTÃO o sistema DEVE representar cada disco como um bloco ASCII horizontal cuja largura é proporcional ao tamanho do disco, centralizado sobre o pino.
2. QUANDO o terminal suporta cores ENTÃO o sistema DEVE colorir cada disco usando códigos de escape ANSI, atribuindo uma cor consistente por tamanho de disco durante a partida.
3. QUANDO o jogo é executado ENTÃO o sistema DEVE usar exclusivamente códigos de escape ANSI para cor (sem `System.Drawing` nem APIs específicas do Windows), de forma a funcionar no terminal do macOS e no container Linux com Mono.
4. SE a saída não for um terminal interativo (por exemplo, saída redirecionada para arquivo/pipe) OU o terminal não suportar cores ENTÃO o sistema DEVE fazer fallback para uma renderização ASCII sem sequências de escape, sem exibir caracteres de controle "crus".
5. QUANDO o jogador desabilita a cor (por exemplo, via variável de ambiente `NO_COLOR` ou uma opção/flag) ENTÃO o sistema DEVE renderizar o tabuleiro em modo monocromático.
6. QUANDO as cores são aplicadas ENTÃO o sistema DEVE restaurar (reset) os atributos de cor do terminal ao final de cada elemento colorido, evitando "vazar" cor para o restante da saída.

### Requirement 10 — Auto execução (resolução automática)

**User Story:** Como jogador, quero poder escolher que o computador resolva o quebra-cabeça sozinho, para que eu possa assistir à solução ótima sendo executada passo a passo.

#### Acceptance Criteria

1. QUANDO o jogador escolhe o modo de auto execução ENTÃO o sistema DEVE resolver o quebra-cabeça usando o algoritmo recursivo clássico da Torre de Hanói, produzindo a sequência ótima de movimentos.
2. QUANDO a auto execução está em andamento ENTÃO o sistema DEVE aplicar um movimento por vez, redesenhando o tabuleiro após cada movimento com uma pausa configurável entre eles, para que a resolução seja visível como uma animação.
3. QUANDO a auto execução realiza um movimento ENTÃO o sistema DEVE respeitar as mesmas regras de validação do jogo manual e incrementar o contador de movimentos.
4. QUANDO a auto execução termina ENTÃO o sistema DEVE ter usado exatamente o número mínimo de movimentos (2^n - 1) e declarar a vitória exibindo o total de movimentos.
5. QUANDO a auto execução está rodando ENTÃO o sistema DEVE permitir que o jogador a interrompa (por exemplo, pressionando uma tecla ou `Ctrl+C`) de forma limpa, retornando ao menu ou encerrando conforme o comando.
6. A geração da sequência de movimentos da solução DEVE ser implementada na camada de domínio, de forma determinística e testável, sem depender de saída de console.

### Requirement 12 — Estilos de renderização (ASCII e blocos)

**User Story:** Como jogador, quero escolher entre o estilo ASCII atual e um estilo de blocos sólidos, para que eu possa jogar com a aparência que preferir.

#### Acceptance Criteria

1. QUANDO o jogo é iniciado ENTÃO o sistema DEVE oferecer dois estilos de renderização do tabuleiro: **ASCII** (o estilo atual, usando o caractere `=` para os discos) e **blocos** (usando caracteres de bloco Unicode, por exemplo `█`, para desenhar discos sólidos).
2. QUANDO o jogador escolhe o estilo ASCII ENTÃO o sistema DEVE renderizar exatamente como o comportamento atual (discos com `=`, mastro com `|`, base com `-`).
3. QUANDO o jogador escolhe o estilo de blocos ENTÃO o sistema DEVE renderizar os discos como blocos sólidos usando caracteres de bloco Unicode, mantendo a mesma geometria (largura proporcional `2*tamanho+1`, centralização e alinhamento das colunas).
4. QUANDO qualquer estilo é usado ENTÃO o sistema DEVE preservar todos os demais elementos do tabuleiro: rótulos A/B/C, base, linha de status (movimentos/mínimo) e a coloração ANSI por tamanho de disco quando a cor estiver habilitada.
5. QUANDO o estilo de blocos é usado ENTÃO o sistema DEVE garantir que a saída Unicode seja emitida corretamente (por exemplo, configurando a codificação de saída do console para UTF-8), sem depender de APIs específicas do Windows.
6. SE a escolha do estilo for inválida ENTÃO o sistema DEVE exibir uma mensagem de erro e solicitar a escolha novamente.
7. A seleção do estilo de renderização DEVE afetar apenas a camada de apresentação, sem alterar a lógica de domínio (regras, estado, solução).
