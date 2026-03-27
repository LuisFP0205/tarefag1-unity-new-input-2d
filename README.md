# Aula0 - Jogo 2D Top Down de Esquiva

## Integrantes do Grupo

| Nome | RA |
|------|----|
|   Anderson Bohnemberger   |  1134706  |
|   Eduardo Morel   |  1134835  |
|   Luis Felipe Pagnussat   |  1134649  |

---

## Visao Geral

Este projeto e um prototipo de jogo `2D top down` feito em `Unity 6`, com foco em movimentacao do jogador e esquiva de projeteis.

A ideia definida para o jogo foi:

- jogador visto de cima
- movimento em `8 direcoes`
- controle apenas por `teclado`
- uso obrigatorio do `Unity New Input System`
- projeteis simples vindo de todos os lados
- loop infinito de sobrevivencia
- jogador nao ataca
- jogador possui `5 vidas`
- ao receber dano, perde `1 vida`
- HUD simples com vidas, tempo sobrevivido e tela de game over

O projeto esta estruturado para servir como base de um jogo no estilo `dodge / survival`, com inspiracao em jogos como `Vampire Survivors`, mas sem combate por enquanto.

---

## O Que Foi Definido

Durante o planejamento, ficaram alinhados os seguintes pontos:

- genero: `top down`
- objetivo: sobreviver desviando de projeteis
- arena: unica
- camera: fixa
- movimento do personagem: livre dentro do quadro visivel da camera
- limites: o personagem nao pode sair da area visivel
- controle: `WASD`
- fisica: `Rigidbody2D`
- personagem: masculino
- sprites do player: imagens da pasta `Assets/Art/Player`

---

## O Que Foi Implementado

### 1. Movimento do Player

Foi criado um controlador do jogador com:

- leitura de input pelo `InputActionAsset`
- movimento em `8 direcoes`
- normalizacao do vetor de movimento
- uso de `Rigidbody2D.MovePosition`
- limitacao da posicao do player dentro da camera
- flip horizontal ao andar para a esquerda
- animacao de caminhada com sprites

Arquivo principal:

- [PlayerController2D.cs](/Assets/Scripts/Runtime/PlayerController2D.cs)

### 2. Sistema de Vida

Foi implementado um sistema de vida do jogador com:

- `5 vidas` maximas
- dano de `1 vida` por acerto
- evento `LivesChanged`
- evento `Died`

Arquivo principal:

- [PlayerHealth.cs](/Assets/Scripts/Runtime/PlayerHealth.cs)

### 3. Sistema de Projetil

Foi criado um prefab de projetil com:

- `Rigidbody2D`
- `Collider2D`
- movimento em direcao ao jogador
- tempo maximo de vida
- aplica dano ao colidir com o player

Arquivos principais:

- [Projectile.cs](/Assets/Scripts/Runtime/Projectile.cs)
- [Projectile.prefab](/Assets/Prefabs/Projectile.prefab)

### 4. Spawner de Projetil

Foi implementado um spawner que:

- cria projeteis ao redor da arena
- escolhe lados aleatorios
- aponta os projeteis para o player
- aumenta a dificuldade com o tempo

Arquivo principal:

- [ProjectileSpawner.cs](/Assets/Scripts/Runtime/ProjectileSpawner.cs)

### 5. HUD

Foi implementado um HUD basico com:

- quadrados de vida no canto superior esquerdo
- texto de tempo sobrevivido
- painel de game over com botao de reinicio

Arquivo principal:

- [GameHUD.cs](/Assets/Scripts/Runtime/GameHUD.cs)

### 6. Gerenciamento Geral do Jogo

Foi criado um gerenciador central para:

- iniciar o jogo
- vincular HUD e player
- acompanhar tempo de sobrevivencia
- escutar eventos de vida
- reiniciar a cena
- parar o spawner quando o jogador morre

Arquivo principal:

- [GameManager.cs](/Assets/Scripts/Runtime/GameManager.cs)

### 7. Camera

Foi implementado um comportamento de camera fixa para manter o enquadramento estavel.

Arquivo principal:

- [FollowCamera2D.cs](/Assets/Scripts/Runtime/FollowCamera2D.cs)

### 8. Setup Automatico da Cena

Foi criado um script de editor para montar ou atualizar rapidamente a cena de teste.

Esse setup cria ou atualiza:

- camera principal
- player
- spawner
- game manager
- arena bounds
- HUD
- prefab do projetil
- referencias serializadas entre objetos

Arquivo principal:

- [DodgePrototypeSetup.cs](/Assets/Scripts/Editor/DodgePrototypeSetup.cs)

---

## Estrutura dos Scripts

### Runtime

- [PlayerController2D.cs](/Assets/Scripts/Runtime/PlayerController2D.cs)
- [PlayerHealth.cs](/Assets/Scripts/Runtime/PlayerHealth.cs)
- [Projectile.cs](/Assets/Scripts/Runtime/Projectile.cs)
- [ProjectileSpawner.cs](/Assets/Scripts/Runtime/ProjectileSpawner.cs)
- [GameHUD.cs](/Assets/Scripts/Runtime/GameHUD.cs)
- [GameManager.cs](/Assets/Scripts/Runtime/GameManager.cs)
- [FollowCamera2D.cs](/Assets/Scripts/Runtime/FollowCamera2D.cs)

### Editor

- [DodgePrototypeSetup.cs](/Assets/Scripts/Editor/DodgePrototypeSetup.cs)

---

## Assets do Personagem

Os sprites do personagem estao na pasta:

- [Assets/Art/Player](/Assets/Art/Player)

Uso dos sprites:

- `Tyer9` como sprite parado
- sequencia de caminhada baseada em `Tyer4` ate `Tyer9`
- flip horizontal ao andar para a esquerda

---

## Input

O projeto usa o `Unity New Input System`.

Asset principal:

- [InputSystem_Actions.inputactions](/Assets/InputSystem_Actions.inputactions)

O arquivo funciona como um dicionario de acoes: define o que o jogador pode fazer (`Move`) e mapeia quais teclas disparam essa acao (`WASD`). O `PlayerController2D` le esse dicionario a cada frame para mover o personagem.

Input definido para o prototipo:

- `WASD` para movimentacao

---

## Como Rodar o Projeto

1. Abrir o projeto no Unity 6.
2. Esperar a recompilacao dos scripts.
3. Executar `Tools > Dodge Prototype > Create Or Refresh`.
4. Salvar a cena com `Ctrl+S`.
5. Abrir [SampleScene.unity](/Assets/Scenes/SampleScene.unity) e clicar em `Play`.

---

## Estado do Projeto

A base de gameplay esta implementada:

- movimentacao em 8 direcoes
- sistema de vida e dano
- spawn de projeteis com dificuldade progressiva
- HUD com vidas e cronometro
- tela de game over com botao de reinicio
