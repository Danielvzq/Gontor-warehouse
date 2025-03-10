"""
robot.py
---------
Representa un robot en el almacén.
"""

from mesa import Agent
from pathfinding import aestrella
import random

class Robot(Agent):
    def __init__(self, model):
        super().__init__(model)

        self.ruta = []
        self.tiene_paquete = False
        self.entregas = 0
        self.historia = []
        self.destinos = []

        # Si el robot empieza en el punto de recogida, recoge un paquete
        if self.pos == self.model.punto_recogida: self.actuar()

        self.elige_destino()


    def update_ruta(self):
        if len(self.ruta) < 1:
            destino = self.destinos[-1]
            self.ruta = aestrella(self.model, self.pos, destino)
    
    def force_update_ruta(self,agente_para_evitar):
        destino = self.destinos[-1]
        self.ruta = aestrella(self.model, self.pos, destino,agente_para_evitar)

    def va_a_chocar(self, destino):
        for agent in self.model.agents:
            if agent != self and agent.ruta and (agent.ruta[0] == destino or agent.pos == destino):
                paquete_diff = (self.tiene_paquete != agent.tiene_paquete) and self.tiene_paquete
                x_diff = (self.pos[0] != agent.pos[0]) and self.pos[0] < agent.pos[0]

                if (paquete_diff or (not paquete_diff and x_diff) \
                    or (not paquete_diff and not x_diff and self.pos[1] < agent.pos[1])):
                    self.force_update_ruta(agent)
                    return False

                return True

        return False

    def step(self):
        self.historia.append(self.pos)
        # print("Ruta:", self.ruta)
        # print("Meta", self.destinos[-1])
        # Si la celda actual es la celda destino, deja o recoge un paquete
        if self.pos == self.destinos[-1]: self.actuar()

        # Si no hay ruta, generar una nueva
        self.update_ruta()

        # Si hara robot en la celda destino, esperar según las reglas:
        if self.va_a_chocar(self.ruta[0]): self.ruta.insert(0, self.pos)

        # Si no hay robot en la celda destino, moverse a la siguiente celda
        if self.ruta:
            self.model.grid.move_agent(self, self.ruta.pop(0))

    
    def actuar(self):
        if self.tiene_paquete:
            self.tiene_paquete = False
            self.entregas += 1
        else:
            self.tiene_paquete = True

        self.elige_destino()
    
    def elige_destino(self):
        if self.tiene_paquete:
            self.destinos.append(random.choice(self.model.puntos_entregas))
        else:
            self.destinos.append(self.model.punto_recogida)