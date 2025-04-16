using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.CullingGroup;
using static UnityEngine.GraphicsBuffer;

#nullable enable

namespace FriendSea.StateMachine
{
    public class LayeredStateMachine<T> where T : class
    {
        List<StateMachine<T>> _layers;
        public IEnumerable<StateMachine<T>> Layers => _layers;
        public IEnumerable<IState<T>> CurrentStates => Layers.Select(l => l.CurrentState);
        public StateMachine<T> DefaultLayer => _layers[0];

        public LayeredStateMachine(IEnumerable<StateMachine<T>> layers)
        {
            if (layers == null) throw new System.ArgumentNullException();
            if (layers.Count() <= 0) throw new System.ArgumentException();
            _layers = layers.ToList();
        }

        public void DoTransition()
        {
            foreach (var layer in _layers)
                layer.DoTransition();
        }

        public void Update(float deltaTime)
        {
            DoTransition();
            foreach (var layer in _layers)
                layer.Update(deltaTime);
        }

        public void ForceState(IState<T> state, int layer = 0) =>
            _layers[layer].ForceState(state);

        public void ForceState(IStateReference<T> state, int layer = 0) =>
            _layers[layer].ForceState(state);
    }
}
