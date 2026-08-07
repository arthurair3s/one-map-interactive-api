# one-map-interactive-api

ASP.NET Core backend do projeto One Piece Interactive Map. Ver `README.md` para stack e como rodar.

## Documentação viva

A documentação canônica do projeto (modelo de dados, regras de negócio, decisões de arquitetura, escopo) vive no repo irmão `one-map-interactive`, pasta `docs/`, esperado no caminho relativo `../docs/` a partir da raiz deste repo (assumindo os repos clonados lado a lado — ver `CLAUDE.md` daquele repo).

**Regra: sempre que uma mudança relevante acontecer aqui** — uma feature nova, uma entidade/coluna nova, uma regra de negócio alterada, uma decisão de arquitetura, uma mudança de contrato de API — o doc correspondente precisa refletir isso. Nunca deixe uma mudança arquitetural ou de regra de negócio sem registro em algum `.md`.

1. **Se `../docs/` existir** (caso normal): atualize o arquivo `.md` relevante lá diretamente, como parte do mesmo trabalho — não como tarefa separada depois.

2. **Se `../docs/` não existir** (repo clonado sozinho, fora da estrutura de irmãos, ou pasta de docs indisponível por qualquer motivo): registre a mudança em `docs-pending/<mesmo-nome-do-arquivo-canônico>.md`, aqui dentro deste repo (ex.: `docs-pending/backend-planning.md`). Crie o arquivo se ainda não existir, usando como base o escopo atual conhecido (se você tiver visto o conteúdo do doc original em algum momento desta sessão, prefira editar a partir dele; senão, comece descrevendo o estado atual da feature/regra do zero). No topo do arquivo, inclua sempre esta linha:
   ```
   <!-- PENDENTE: mesclar em ../one-map-interactive/docs/<nome>.md e apagar este arquivo -->
   ```
   Isso torna a reconciliação inequívoca mesmo em uma sessão futura sem este contexto.

3. **No início de qualquer sessão de trabalho neste repo**, verifique proativamente se `../docs/` voltou a existir e se há arquivos em `docs-pending/`. Se sim: mescle cada `docs-pending/<nome>.md` no `../docs/<nome>.md` correspondente, apague o arquivo pendente, e remova a pasta `docs-pending/` se ficar vazia. Não espere o usuário pedir — isso é parte do fluxo normal.

`docs-pending/` é rastreado no git (não é ignorado) — o conteúdo é real e não pode se perder entre sessões só porque ainda não foi reconciliado.
