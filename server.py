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

async def run_simulation(entorno, entregas_deseados):
    while entorno.entregas < entregas_deseados:
        entorno.step()

async def handler(websocket):
    async for message in websocket:
        print("Received:", message)
        data = json.loads(message)
        # Default: 1 robot, 30x53 almacen 
        num_agents = int(data.get("num_agents", 1))
        pos_iniciales = data.get("initial_positions", [])
        width = int(data.get("width", 30))
        length = int(data.get("length", 53))
        obstaculos = data.get("obstacles", [])
        puntos_recogidas = data.get("puntos_recogidas", [(0,0)])
        metas = data.get("puntos_entregas", [(width-1, length-1)])
        entregas_deseados = data.get("deliveries", 100)
        generar_txt = data.get("generate_txt", False)
        entorno = Entorno(num_agents, width, length,
                          puntos_recogidas=puntos_recogidas,
                          pos_iniciales=pos_iniciales,
                          obstaculos=obstaculos,
                          puntos_entregas=metas)

        # Run the simulation in an async task
        simulation_task = asyncio.create_task(run_simulation(entorno, entregas_deseados))

        # Keep the connection alive while the simulation runs
        while not simulation_task.done():
            await asyncio.sleep(1)

        # Wait for the simulation to complete
        await simulation_task

        rutas = []
        metas = []
        for agent in entorno.agents:
            rutas.append(agent.historia)
            metas.append(agent.destinos)
        # print(len(rutas), len(metas))
        response = {
            "rutas": rutas,
            "metas": metas
        }

        await websocket.send(json.dumps(response))

        if generar_txt:
            # Crear un archivo de texto con las rutas
            # de la forma:
            # x1,x2,x3,x4
            # y1,y2,y3,y4
            for i, agent in enumerate(entorno.agents):
                with open(f"./rutas/rutas{i}.txt", "w") as f:
                    # En el robotario, (0,0) esta en el centro, restar el medio del ancho y alto del almacen
                    x = [x-(entorno.grid.width/2.0) for x,y in agent.historia]
                    y = [y-(entorno.grid.height/2.0) for x,y in agent.historia]
                    f.write(",".join(map(str, x)) + "\n")
                    f.write(",".join(map(str, y)) + "\n")


async def main():
    # Esperar a que el cliente se conecte
    async with serve(handler, "localhost", 8765) as server:
        await server.serve_forever()

if __name__ == "__main__":
    asyncio.run(main())