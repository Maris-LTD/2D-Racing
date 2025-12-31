using UnityEngine;

namespace Game.Car.Display
{
    public class CarView : CarModule, ICarDisplayModule
    {
        private SpriteRenderer _spriteRenderer;
        private int _currentCarIndex;

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

            _spriteRenderer = _controller.CarSpriteRenderer;
            if (_spriteRenderer == null)
            {
                return;
            }

            var carData = _controller.CarDataList.carDataList[_currentCarIndex];
            if (carData != null)
            {
                _spriteRenderer.sprite = carData.carSprite;
            }
        }
    }
}
