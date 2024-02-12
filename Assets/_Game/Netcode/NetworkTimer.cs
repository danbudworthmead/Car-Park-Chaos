namespace _Game.Netcode
{
    public class NetworkTimer
    {
        public float MinTimeBetweenTicks { get; }
        public int CurrentTick { get; private set; }
        private float _timer;
        
        public NetworkTimer(float serverTickRate)
        {
            MinTimeBetweenTicks = 1 / serverTickRate;
        }

        public void Update(float deltaTime)
        {
            _timer += deltaTime;
        }

        public bool ShouldTick()
        {
            if (_timer >= MinTimeBetweenTicks)
            {
                _timer -= MinTimeBetweenTicks;
                CurrentTick++;
                return true;
            }

            return false;
        }
    }
}
