# Implementación del algoritmo de búsqueda A*, generado por Github Copilot
# acepta un entorno y dos puntos de inicio y fin
# devuelve una lista de puntos que representan el camino más corto
def aestrella(entorno, inicio, fin, agentes_para_evitar=None):
    class Nodo:
        def __init__(self, pos, padre, goal):
            self.pos = pos
            self.padre = padre
            self.g = 0 if not padre else padre.g + 1
            self.h = abs(pos[0] - goal[0]) + abs(pos[1] - goal[1])
            self.x = self.evitar()
            self.f = self.g + self.h + self.x

        def evitar(self):
            # Si la celda actual está ocupada por un obstáculo, asignar un costo muy alto
            costo_obstaculo = 1000000 if (self.pos in entorno.obstaculos) else 0

            costo_agentes = 0
            for agent in entorno.agents:
                if self.pos in agent.ruta:
                    costo_agentes += 10

            if agentes_para_evitar:
                for agent in agentes_para_evitar:
                    if agent.pos == self.pos:
                        costo_agentes = 1000000
                        break

            return costo_obstaculo + costo_agentes

        def __eq__(self, other):
            return self.pos == other.pos

        def __hash__(self):
            return hash(self.pos)

    # Inicializar la lista de nodos abiertos y cerrados
    abiertos = []
    cerrados = []
    # Crear el nodo inicial
    nodo = Nodo(inicio, None, fin)
    # Añadir el nodo inicial a la lista de nodos abiertos
    abiertos.append(nodo)
    # Mientras haya nodos abiertos
    while abiertos:
        # Seleccionar el nodo con el menor valor de f
        nodo_actual = abiertos[0]
        for nodo in abiertos:
            if nodo.f < nodo_actual.f:
                nodo_actual = nodo
        # Mover el nodo actual de la lista de abiertos a cerrados
        abiertos.remove(nodo_actual)
        cerrados.append(nodo_actual)
        # Si el nodo actual es el nodo final
        if nodo_actual.pos == fin:
            # Reconstruir el camino y devolverlo
            camino = []
            while nodo_actual:
                camino.append(nodo_actual.pos)
                nodo_actual = nodo_actual.padre
            
            # print("Camino:", camino[::-1])
            return camino[::-1][1:]
        # Generar los nodos sucesores
        for vecino in entorno.grid.get_neighborhood(nodo_actual.pos, moore=False, include_center=False):
            # Crear el nodo sucesor
            sucesor = Nodo(vecino, nodo_actual, fin)
            # Si el nodo sucesor está en la lista de nodos cerrados, ignorarlo
            if sucesor in cerrados:
                continue
            # Calcular el valor de g del nodo sucesor
            sucesor.g = nodo_actual.g + 1
            # Si el nodo sucesor no está en la lista de nodos abiertos, añadirlo
            if sucesor not in abiertos:
                abiertos.append(sucesor)
            # Si el nodo sucesor está en la lista de nodos abiertos y su valor de g es mayor que el del nodo actual, ignorarlo
            elif sucesor.g >= nodo_actual.g:
                continue
            # Actualizar el nodo sucesor en la lista de nodos abiertos
            abiertos[abiertos.index(sucesor)] = sucesor
    # Si no se ha encontrado un camino, devolver una lista vacía
    return []