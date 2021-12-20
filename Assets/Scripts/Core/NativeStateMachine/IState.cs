using System;

namespace Core.NativeStateMachine
{
    public interface IState
    {
        event Action Enter;  
        event Action Exit;  
        
        int GetHash();
        void Tick();
        void OnEnter();
        void OnExit();
    }
}