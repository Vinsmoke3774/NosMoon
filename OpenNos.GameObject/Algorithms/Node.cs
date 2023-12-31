﻿using Priority_Queue;

namespace OpenNos.GameObject.Algorithms
{
    public class Node<T> : FastPriorityQueueNode
    {
        public readonly T Position;
        public readonly int Previous;
        public readonly int Cost;
        public readonly float Heuristic;
        public readonly int Index;

        public Node(T position, int index, int cost = 0, float heuristic = 0, int previous = 0)
        {
            Position = position;
            Index = index;
            Heuristic = heuristic;
            Cost = cost;
            Previous = previous;
        }
    }
}