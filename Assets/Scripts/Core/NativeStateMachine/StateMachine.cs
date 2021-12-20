using System;
using System.Collections.Generic;

namespace Core.NativeStateMachine
{
    public class StateMachine
    {
        private IState _currentState;
        private IState _savedState;
        
        private Dictionary<Type, List<Transition>> _transitions = new Dictionary<Type, List<Transition>>();
        private List<Transition> _currentTransitions = new List<Transition>();
        private List<Transition> _anyTransitions = new List<Transition>();
        
        private static List<Transition> EmptyTransitions = new List<Transition>(0);

        private bool _isActive = false;
        private bool _isTransitionsEnabled = true;

        public bool IsActive => _isActive;

        public IState SavedState => _savedState;

        public void Stop()
        {
            _isActive = false;
        }

        public void Start()
        {
            _isActive = true;
        }

        public void EnableTransitions()
        {
            _isTransitionsEnabled = true;
        }

        public void DisableTransitions()
        {
            _isTransitionsEnabled = false;
        }
        
        public void Tick()
        {
            if (IsActive == false)
                return;

            if (_isTransitionsEnabled)
            {
                var transition = GetTransition();
                if (transition != null)
                    SetState(transition.To);
            }

            _currentState?.Tick();
        }
        
        public void SetState(IState state)
        {
            if (state == _currentState)
                return;

            _currentState?.OnExit();
            _currentState = state;

            _transitions.TryGetValue(_currentState.GetType(), out _currentTransitions);
            if (_currentTransitions == null)
                _currentTransitions = EmptyTransitions;
            
            _currentState?.OnEnter();
        }

        public void SaveState()
        {
            _savedState = _currentState;
        }

        public void LoadState()
        {
            if (_savedState == null)
                return;
            
            SetState(_savedState);
        }

        public string GetCurrentStateName()
        {
            return _currentState is null == false ? _currentState.GetType().Name : string.Empty;
        }

        public void AddTransition(IState from, IState to, Func<bool> predicate)
        {
            if (_transitions.TryGetValue(from.GetType(), out var transitions) == false)
            {
                transitions = new List<Transition>();
                _transitions[from.GetType()] = transitions;
            }
            
            transitions.Add(new Transition(to, predicate));
        }

        public void AddAnyTransition(IState state, Func<bool> predicate)
        {
            _anyTransitions.Add(new Transition(state, predicate));
        }

        public void ClearTransitions()
        {
            _transitions.Clear();
            _currentTransitions.Clear();
            _anyTransitions.Clear();
        }

        private class Transition
        {
            public Func<bool> Condition { get; }
            public IState To { get; }

            public Transition(IState to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }
        }

        private Transition GetTransition()
        {
            foreach(var transition in _anyTransitions)
                if (transition.Condition())
                    return transition;
            
            foreach(var transition in _currentTransitions)
                if (transition.Condition())
                    return transition;

            return null;
        }
    }
}
