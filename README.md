# Gontor Warehouse - "Digital Twins"

🚀 Technologies Used
Python (Backend and agent logic)
Unity (Visualization and simulation)
WebSockets (Real-time communication)
C# (Agent control in Unity)

🛠️ Installation & Execution
1️⃣ Setting up the Server (Python)
Make sure you have Python 3.8+ installed.

Install dependencies with:
pip install -r requirements.txt

Run the server:
python server.py

2️⃣ Setting up Unity
Open Unity and load the project from https://drive.google.com/file/d/1SnAOX6mvl5xLHt8235MFrFU_XHP41_6_/view?usp=drive_link.
Ensure that the SocketClient.cs and Websocket.cs scripts are correctly assigned to the agents.
Run the simulation in Unity.

📬 Communication Between Server and Unity
Unity sends initial positions and relevant events to the server.
Python calculates agent routes and decisions.
WebSockets facilitate real-time data transmission.

✨ Key Features
✔️ Agent simulation in Unity
✔️ Optimized pathfinding in Python
✔️ Real-time communication with WebSockets
✔️ Integration for digital twins

📝 Authors
Juan Daniel Vázquez Alonso
Aidan Russel Parkhurst
Bernardo Caballero Zambrano
