"""
server.py
---------
Script principal para ejecutar la simulación.
Simula el entorno y comunica con el cliente de unity por websockets.
"""
import asyncio
import time
from websockets.asyncio.server import serve
import json

from entorno import Entorno

async def handler(websocket):
    try:
        async for message in websocket:
            print(f"Mensaje recibido: {message}")  # Añadir esta línea para ver el mensaje recibido
            data = json.loads(message)
            
            # Default: 1 robot, 30x53 almacen 
            num_agents = int(data.get("num_agents", 1))
            width = int(data.get("width", 30))
            length = int(data.get("length", 53))
            entorno = Entorno(num_agents, width, length)

            rutas = []
            for agent in entorno.agents:
                rutas.append(agent.ruta)
            response = {"rutas": rutas}

            await websocket.send(json.dumps(response))

            for i in range(50):
                entorno.step()
    except json.JSONDecodeError as e:
        print(f"Error de decodificación JSON: {e}")


async def main():
    # Esperar a que el cliente se conecte
    async with serve(handler, "localhost", 8765) as server:
        await server.serve_forever()

if __name__ == "__main__":
    asyncio.run(main())