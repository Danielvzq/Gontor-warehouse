"""
robot.py
---------
Representa un robot en el almacén.
"""

from mesa import Agent
from pathfinding import aestrella

class Robot(Agent):
    ruta = []
    tiene_paquete = False
    paquetes = 0

    def __init__(self, model):
        self.model = model
        super().__init__(model)

    def update_ruta(self):
        if not self.ruta:
            destino = self.model.punto_entrega if self.tiene_paquete else self.model.punto_recogida
            self.ruta = aestrella(self.model, self.pos, destino)

    def va_a_chocar(self, destino):
        for agent in self.model.agents:
            if agent != self and agent.ruta and agent.ruta[0] == destino:
                return agent.pos[0] < self.pos[0]
        return False

    def step(self):
        # Si no hay ruta, generar una nueva
        self.update_ruta()

        # Si la celda actual es la siguiente celda de la ruta, quitarla de la ruta 
        if self.pos == self.ruta[0]: self.ruta.pop(0)

        # Si hara robot en la celda destino, esperar según las reglas:
        if self.va_a_chocar(self.ruta[0]):
            self.ruta.insert(0, self.pos)

        # Si no hay robot en la celda destino, moverse a la siguiente celda
        self.model.grid.move_agent(self, self.ruta.pop(0))

        # Si la celda actual es la celda destino, deja o recoge un paquete
        if self.pos == self.model.punto_recogida:
            self.tiene_paquete = True
            print("Recogiendo paquete")
        elif self.pos == self.model.punto_entrega:
            self.tiene_paquete = False
            self.paquetes += 1
            print("Dejando paquete:", self.paquetes)