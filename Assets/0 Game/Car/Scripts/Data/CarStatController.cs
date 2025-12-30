namespace Game.Car.Data
{
    public class CarStatController : CarModule, ICarStatModule
    {
        private int _currentCarIndex;
        private float _speed;
        private float _acceleration;
        
        public override void OnCarInit()
        {
            _currentCarIndex = _controller.CurrentCarIndex;
            _speed = _controller.CarDataList.carDataList[_currentCarIndex].maxSpeed;
            _acceleration = _controller.CarDataList.carDataList[_currentCarIndex].acceleration;
        }

        public float GetSpeed(){
            return _speed;
        }

        public float GetAcceleration(){
            return _acceleration;
        }
    }
}
