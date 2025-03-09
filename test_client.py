"""
test_client.py
---------
Cliente de prueba para el servidor de simulación.
Conecta al servidor por websockets y recibe mensajes.
"""

import asyncio
import websockets
import json

async def client():
    uri = "ws://localhost:8765"
    async with websockets.connect(uri) as websocket:
        num_agents = input("How many agents do you want? (Default 1) ")
        width = input("How wide do you want the warehouse? (Default 30) ")
        length = input("How long do you want the warehouse? (Default 53) ")
        data = {
            "num_agents": num_agents if num_agents.strip() != "" else 1,
            "width": width if width.strip() != "" else 30,
            "length": length if length.strip() != "" else 53
            }
        await websocket.send(json.dumps(data))
        response = await websocket.recv()
        print(response)

if __name__ == "__main__":
    asyncio.run(client())