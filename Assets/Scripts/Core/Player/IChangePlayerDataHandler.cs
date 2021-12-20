namespace Core.Player
{
    public interface IChangePlayerDataHandler
    {
        public void OnChangeWheelHandle(MonoWheel wheel);
        public void OnChangeCharacterHandle(Character character);
        public void OnChangeReady();
        public void OnChangePreparing();
    }
}
