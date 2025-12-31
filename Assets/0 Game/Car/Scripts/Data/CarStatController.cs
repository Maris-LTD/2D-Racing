namespace Game.Car.Data
{
    public class CarStatController : CarModule, ICarStatModule
    {
        private int _currentCarIndex;
        private float _speed;
        private float _acceleration;
        
        public override void OnCarInit()
        {
            if (_controller == null || _controller.CarDataList == null || 
                _controller.CarDataList.carDataList == null)
            {
                return;
            }

            _currentCarIndex = _controller.CurrentCarIndex;
            
            if (_currentCarIndex < 0 || _currentCarIndex >= _controller.CarDataList.carDataList.Count)
            {
                return;
            }

            var carData = _controller.CarDataList.carDataList[_currentCarIndex];
            if (carData == null)
            {
                return;
            }

            _speed = carData.maxSpeed;
            _acceleration = carData.acceleration;
        }

        public float GetSpeed(){
            return _speed;
        }

        public float GetAcceleration(){
            return _acceleration;
        }
    }
}
