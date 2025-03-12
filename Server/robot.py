"""
robot.py
---------
Representa un robot en el almacén.
"""

from mesa import Agent
from pathfinding import aestrella
import random

class Robot(Agent):
    def __init__(self, model, id):
        super().__init__(model)

        self.id = id
        self.ruta = []
        self.tiene_paquete = False
        self.entregas = 0
        self.historia = []
        self.destinos = []

        # Si el robot empieza en el punto de recogida, recoge un paquete
        if self.pos in self.model.puntos_recogidas: self.actuar()

        self.elige_destino()


    def update_ruta(self):
        # Si no hay ruta, generar una nueva
        if not self.ruta:
            destino = self.destinos[-1]
            self.ruta = aestrella(self.model, self.pos, destino)
            if not self.ruta:
                print(f"Robot {self.id} está atrapado")
                return
        else:   
            # Si hay ruta, y va a chocar con otro robot, espera o cambia de ruta
            if self.va_a_chocar(self.ruta[0]): self.ruta.insert(0, self.pos)
    
    def force_update_ruta(self,agentes_para_evitar):
        destino = self.destinos[-1]
        self.ruta = aestrella(self.model, self.pos, destino, agentes_para_evitar)

    def va_a_chocar(self, destino):
        agentes_para_evitar = []
        for agent in self.model.agents:
            if agent != self and (agent.pos == destino or (agent.ruta and agent.ruta[0] == destino)):
                agentes_para_evitar.append(agent)
        
        if agentes_para_evitar:
            # print(f"Robot {self.id} va a chocar con", [a.id for a in agentes_para_evitar])
            otro_robot_es_especial = False
            for agent in agentes_para_evitar:
                otro_robot_es_especial = otro_robot_es_especial or (agent.pos in self.model.puntos_recogidas or (agent.pos in self.model.puntos_entregas))
            punto_especial = (self.pos in self.model.puntos_recogidas) or (self.pos in self.model.puntos_entregas)
            especial = punto_especial and not otro_robot_es_especial
            # print(f"Robot {self.id} es especial: {especial}")
            if especial or (not especial and self.id < min([agent.id for agent in agentes_para_evitar])):
                # print(f"Robot {self.id} recalculated!")
                self.force_update_ruta(agentes_para_evitar)
                return False

            # Por los demas, esperar
            return True

        # No va a chocar
        return False


    def step(self):
        self.historia.append(self.pos)
        # print("Robot:",self.id,"Pos:", self.pos)
        # print("Robot:",self.id,"Tiene paquete:", self.tiene_paquete)
        # print("Robot:",self.id,"Ruta:", self.ruta)
        # print("Robot:",self.id,"Meta", self.destinos[-1])

        # Si no hay ruta, generar una nueva
        self.update_ruta()

        # Si no hay ruta, el robot está atrapado
        if not self.ruta:
            return
 
        self.model.grid.move_agent(self, self.ruta.pop(0))

        # Si la celda actual es la celda destino, deja o recoge un paquete
        if self.pos == self.destinos[-1]: self.actuar()
    
    def actuar(self):
        if self.tiene_paquete:
            self.tiene_paquete = False
            self.entregas += 1
            self.model.entregas += 1
            print(self.model.entregas)
        else:
            self.tiene_paquete = True

        self.elige_destino()
    
    def elige_destino(self):
        if self.tiene_paquete:
            self.destinos.append(random.choice(self.model.puntos_entregas))
        else:
            self.destinos.append(random.choice(self.model.puntos_recogidas))
        
        if self.pos: self.update_ruta()