# Unity New Input System — Como funciona neste projeto

---

## A ideia central

O New Input System trabalha com **acoes**.

Em vez de o codigo perguntar a cada frame:
> "A tecla W esta pressionada?"

Voce define o que o jogador pode **fazer**:
> "O jogador pode se **Mover**"

E ai voce diz quais teclas disparam essa acao.

---

## O dicionario de acoes

O arquivo `InputSystem_Actions.inputactions` e o dicionario do projeto.

A direcao de leitura e:

```
ACAO  →  TECLAS

"Move"  →  W, A, S, D
```

Nao o contrario. O codigo nunca pergunta se W foi pressionado.
Ele pergunta se a acao **Move** foi acionada.

Isso significa que se um dia quiser trocar WASD por setas do teclado,
ou adicionar suporte a controle, voce muda so nesse arquivo.
**O codigo do jogo nao precisa mudar nada.**

---

## O que tem dentro do arquivo

O arquivo esta organizado em dois grupos chamados **Action Maps**:

**Player** — acoes do jogador durante o jogo
- `Move` → WASD, setas do teclado, analogico do controle

**UI** — acoes de interface (navegacao de menus)
- Navigate, Submit, Cancel, Click, etc.

Neste projeto so o `Move` do grupo `Player` e usado pelo jogo.
O restante veio junto por ser o template padrao do Unity.

---

## Quem le esse dicionario

Dois leitores usam o mesmo arquivo:

**1. PlayerController2D** — le o `Move` para mover o personagem

**2. InputSystemUIInputModule** — le as acoes de UI para navegacao de interface

---

## Como o PlayerController2D le na pratica

### Passo 1 — Recebe o arquivo via Inspector
```cs
[SerializeField] private InputActionAsset inputActions;
```
O arquivo `.inputactions` e arrastado para esse campo no Unity.

### Passo 2 — Localiza a acao pelo nome
```cs
_moveAction = inputActions.FindAction("Player/Move", true);
```
`"Player/Move"` significa: dentro do grupo `Player`, a acao `Move`.

### Passo 3 — Liga e desliga junto com o objeto
```cs
private void OnEnable()  { _moveAction.Enable();  }
private void OnDisable() { _moveAction.Disable(); }
```
A acao precisa ser habilitada para comecar a capturar input.

### Passo 4 — Le o valor a cada frame
```cs
_moveInput = _moveAction.ReadValue<Vector2>();
```
O valor retornado e um `Vector2` com a direcao do movimento:
```
W         →  (0,  1)
S         →  (0, -1)
A         →  (-1, 0)
D         →  ( 1, 0)
W + D     →  ( 1,  1)  ← diagonal
```

### Passo 5 — Normaliza o vetor diagonal
```cs
if (_moveInput.sqrMagnitude > 1f)
{
    _moveInput.Normalize();
}
```
Sem essa linha, andar na diagonal seria mais rapido do que andar reto,
porque o vetor `(1, 1)` tem magnitude maior que `(1, 0)`.
A normalizacao garante velocidade igual em todas as direcoes.

---

## Resumo do fluxo completo

```
InputSystem_Actions.inputactions
         |
         |  Action Map: Player
         |  Action: Move
         |  Bindings: W A S D
         |
         v
PlayerController2D.ReadValue<Vector2>()
         |
         v
Rigidbody2D.MovePosition()
         |
         v
Personagem se move na tela
```
