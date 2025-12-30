using UnityEngine;

namespace Game.Car.Display
{
    public class CarView : CarModule, ICarDisplayModule
    {
        private SpriteRenderer _spriteRenderer;
        private int _currentCarIndex;

        public override void OnCarInit()
        {
            _currentCarIndex = _controller.CurrentCarIndex;
            _spriteRenderer = _controller.CarSpriteRenderer;
            _spriteRenderer.sprite = _controller.CarDataList.carDataList[_currentCarIndex].carSprite;
        }
    }
}
