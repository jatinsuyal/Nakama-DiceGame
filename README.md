# 🎲 Dice Game

## 💬 What is this?
Dice Game is a 2-player online game created using the Unity game engine, designed to demonstrate the capabilities of the Nakama Open Source Game Server.

It showcases the following Nakama features:
- Device Authentication
- Matchmaking
- Realtime Multiplayer

The gameplay mechanics involve players taking turns to roll a dice with synchronized results ensuring fairness and real-time interaction.

## 🛠️ Getting Started
To set up the game, follow these steps:

1. **Install Docker Desktop**:
   Download and install Docker Desktop from [Docker's official website](https://www.docker.com/products/docker-desktop).

2. **Set up the Game Server**:
   - Locate the `docker-compose.yml` file in the root directory of the project.
   - Open a terminal or command prompt in this directory and run:
     ```
     docker-compose up
     ```
   This command will start the Nakama Game Server and an instance of CockroachDB, which Nakama uses for data storage.

3. **Developing with Unity**:
   - The game is developed using Unity 2022.3.23f1, and should be compatible with any version of Unity 2022.3.
   - Open the Dice Game project in Unity by navigating to `File -> Open Project`.
   - Go to `File -> Build Settings` and build the game for your chosen platform.
   - Launch two instances of the game to test multiplayer functionality locally.

## 🕹️ Controls
- **Spacebar**: Roll the dice.
- **Mouse Click**: Interact with UI elements.

## 📜 License
This project is licensed under the Apache 2.0 License. Artwork and additional media may be covered under different licenses.

## 🎉 Development
This project was developed by Jatin Suyal as part of a demonstration on how to implement multiplayer games using Nakama and Unity.
