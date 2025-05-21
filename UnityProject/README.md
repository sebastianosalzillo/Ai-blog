# Unity AI Shopkeeper Chat Project

## Project Overview

This project demonstrates a basic 3D environment in Unity where a player can interact with an AI-powered shopkeeper. The shopkeeper's dialogue is generated using Google's Gemini API, allowing for dynamic and engaging conversations. The interaction is triggered by proximity, and the player communicates through a simple UI panel.

## Features

*   **AI-Powered Dialogue:** Shopkeeper responses are generated dynamically by the Google Gemini API.
*   **3D First-Person Player Controller:** Basic movement (WASD/Arrows) and mouse look.
*   **Proximity-Based Interaction:** Player must be close to the shopkeeper to initiate conversation.
*   **UI for Chat:** A simple UI panel for typing input and viewing responses.
*   **Configurable AI Persona:** The shopkeeper's personality and context can be defined through a base prompt.
*   **Clear Separation of Concerns:** Different scripts handle player movement, AI service communication, AI logic, UI, and interaction management.

## Core Scripts

Here's a brief description of the role of each main C# script:

*   **`PlayerMovement.cs` (`Assets/Scripts/PlayerMovement.cs`)**
    *   Handles first-person player movement (walking, strafing) and mouse look (camera rotation).
    *   Designed to be attached to the player GameObject.

*   **`GoogleAIService.cs` (`Assets/Scripts/GoogleAIService.cs`)**
    *   Provides a generic service to communicate with the Google Gemini API.
    *   Handles constructing API requests, sending them, and parsing the JSON responses.
    *   It is used by `ShopkeeperAI.cs` to fetch dialogue.

*   **`ShopkeeperAI.cs` (`Assets/Scripts/ShopkeeperAI.cs`)**
    *   Manages the shopkeeper's AI logic and personality.
    *   Constructs prompts for the `GoogleAIService` by combining a base persona prompt with player input.
    *   Stores the API key and desired AI model name.

*   **`ShopkeeperUI.cs` (`Assets/Scripts/UI/ShopkeeperUI.cs`)**
    *   Controls the chat user interface, including the input field for player messages, the text area for shopkeeper responses, and the submit button.
    *   Manages the visibility of the chat panel.

*   **`ShopInteractionManager.cs` (`Assets/Scripts/Interaction/ShopInteractionManager.cs`)**
    *   Manages the interaction between the player and the shopkeeper.
    *   Detects player proximity and displays an interaction prompt (e.g., "Press E to Talk").
    *   Handles key presses to open and close the chat UI via `ShopkeeperUI.cs`.

## Setup Instructions

Follow these steps to set up the project in the Unity Editor:

1.  **Create a New Unity Project:**
    *   Open Unity Hub and create a new project using the "3D Core" template.

2.  **Import Scripts:**
    *   Download all the C# scripts.
    *   In your Unity project's `Assets` folder, create a new folder named `Scripts`.
    *   Recreate the subdirectory structure within `Assets/Scripts/` as provided:
        *   `Assets/Scripts/PlayerMovement.cs`
        *   `Assets/Scripts/GoogleAIService.cs`
        *   `Assets/Scripts/ShopkeeperAI.cs`
        *   `Assets/Scripts/UI/ShopkeeperUI.cs`
        *   `Assets/Scripts/Interaction/ShopInteractionManager.cs`
    *   Drag the corresponding scripts into these folders.

3.  **Scene Setup (Using `ShopSceneSetup.txt`):**
    *   A file named `ShopSceneSetup.txt` was created in a previous step (`UnityProject/Assets/Scenes/ShopSceneSetup.txt`). This file contains detailed instructions for setting up a basic shop scene, including:
        *   Creating a floor and walls.
        *   Adding a "ShopCounter" cube.
        *   Setting up a "Player" capsule with a `CharacterController` and a child "Main Camera".
        *   Creating a "Shopkeeper" capsule.
    *   Follow these instructions carefully to build the basic environment.
    *   **Important:** Ensure your player GameObject is tagged with "Player". Select the Player GameObject, and in the Inspector, find the "Tag" dropdown and select "Player" (or add it if it doesn't exist).

4.  **Attach Scripts to GameObjects & Configure Inspector Fields:**

    *   **Player GameObject:**
        *   Attach `PlayerMovement.cs`.
            *   **Player Body:** Drag the Player GameObject itself to this field.
            *   **Player Camera:** Drag the Main Camera (which should be a child of the Player) to this field.

    *   **Shopkeeper GameObject:**
        *   Attach `GoogleAIService.cs`.
            *   *(No Inspector assignments needed for this script directly, but `ShopkeeperAI` will use it).*
        *   Attach `ShopkeeperAI.cs`.
            *   **Ai Service:** Drag the Shopkeeper GameObject (which has `GoogleAIService.cs`) to this field.
            *   **Api Key:** **Enter your Google AI API Key here.** (See Configuration section).
            *   **Model Name:** (Optional) Change if you want to use a different model like "gemini-pro". Defaults to "gemini-1.5-flash".
            *   **Base Prompt:** Review and customize the default prompt to define your shopkeeper's personality.
        *   Attach `ShopInteractionManager.cs`.
            *   **Shopkeeper UI Script:** (This will be assigned after setting up the Canvas UI).
            *   **Player Transform:** Drag your Player GameObject to this field.
            *   **Interaction Prompt Text:** (This will be assigned after setting up the Canvas UI).
            *   **Interaction Range:** Adjust as needed (default is 3 units).
            *   **Interaction Key:** Change if desired (default is E).
            *   **Close UI Key:** Change if desired (default is Escape).

    *   **Canvas UI Setup:**
        *   Right-click in the Hierarchy -> UI -> Canvas. Name it "ShopCanvas" (or similar).
        *   Ensure it has an EventSystem (Unity usually adds this automatically with a Canvas).
        *   **Create Chat Panel:**
            *   Create a Panel as a child of the Canvas: Right-click "ShopCanvas" -> UI -> Panel. Name it "ChatPanel". This will be the `Main Chat Panel` for `ShopkeeperUI`.
            *   Anchor it to the bottom or desired position. Resize as needed.
        *   **Add UI Elements as Children of "ChatPanel":**
            *   **Input Field:** Right-click "ChatPanel" -> UI -> InputField (or InputField - TextMeshPro). Name it "PlayerInputField".
            *   **Shopkeeper Response Text:** Right-click "ChatPanel" -> UI -> Text (or Text - TextMeshPro). Name it "ShopkeeperResponseText". Adjust its size to allow multiple lines.
            *   **Submit Button:** Right-click "ChatPanel" -> UI -> Button (or Button - TextMeshPro). Name it "SubmitButton". Change its child Text to "Submit".
            *   **(Optional) Status Text:** Right-click "ChatPanel" -> UI -> Text (or Text - TextMeshPro). Name it "StatusText". Place it appropriately.
        *   **Create Interaction Prompt Text:**
            *   Right-click "ShopCanvas" (but *not* as a child of "ChatPanel") -> UI -> Text (or Text - TextMeshPro). Name it "InteractionPrompt".
            *   Center it on the screen or place it where interaction prompts are desired.
            *   This text will be controlled by `ShopInteractionManager`.
        *   **Attach `ShopkeeperUI.cs`:**
            *   Select the "ShopCanvas" GameObject (or create an empty GameObject like "UIManager" as a child of the Canvas and attach the script there).
            *   Attach `ShopkeeperUI.cs` to it.
                *   **Shopkeeper AI:** Drag your Shopkeeper GameObject to this field.
                *   **Main Chat Panel:** Drag the "ChatPanel" GameObject (the Panel you created) to this field.
                *   **Player Input Field:** Drag your "PlayerInputField" UI element to this field.
                *   **Shopkeeper Response Text:** Drag your "ShopkeeperResponseText" UI element to this field.
                *   **Submit Button:** Drag your "SubmitButton" UI element to this field.
                *   **Status Text:** (Optional) Drag your "StatusText" UI element to this field.

    *   **Final Link for `ShopInteractionManager`:**
        *   Select the Shopkeeper GameObject again.
        *   In the `ShopInteractionManager` component:
            *   **Shopkeeper UI Script:** Drag the "ShopCanvas" (or "UIManager") GameObject (which has `ShopkeeperUI.cs`) to this field.
            *   **Interaction Prompt Text:** Drag the "InteractionPrompt" UI Text element to this field.

5.  **Save the Scene:**
    *   File -> Save Scene. Name it something like "ShopScene".

## Configuration

*   **Google AI API Key:**
    *   This is the most crucial step for the AI to function.
    *   Select the Shopkeeper GameObject.
    *   In the Inspector, find the `Shopkeeper AI` component.
    *   Paste your Google AI API Key into the `Api Key` field. You can obtain an API key from [Google AI Studio](https://aistudio.google.com/app/apikey).
*   **Shopkeeper Persona (`basePrompt`):**
    *   Select the Shopkeeper GameObject.
    *   In the `Shopkeeper AI` component, modify the `Base Prompt` text area. This text defines the shopkeeper's personality, backstory, items they sell, and how they should respond. Be descriptive!
*   **Interaction Settings:**
    *   Select the Shopkeeper GameObject.
    *   In the `Shop Interaction Manager` component:
        *   `Interaction Range`: Adjust the distance the player needs to be within to talk to the shopkeeper.
        *   `Interaction Key`: Change the key used to start the conversation (default: E).
        *   `Close UI Key`: Change the key used to close the chat window (default: Escape).

## Running the Game

1.  Open your configured scene (e.g., "ShopScene").
2.  Press the Play button in the Unity Editor.
3.  Use WASD or Arrow keys to move the player. Use the mouse to look around.
4.  Approach the shopkeeper. When you are within the `Interaction Range`, the "Press E to Talk" (or your configured key/text) prompt should appear.
5.  Press the `Interaction Key` (default E). The chat UI panel should appear.
6.  Type your message in the input field and click "Submit" or press Enter (if the InputField is configured for submit on Enter).
7.  The shopkeeper's AI-generated response will appear in the response text area.
8.  Press the `Close UI Key` (default Escape) to close the chat panel.

This setup should provide a functional AI shopkeeper experience in your Unity project.
